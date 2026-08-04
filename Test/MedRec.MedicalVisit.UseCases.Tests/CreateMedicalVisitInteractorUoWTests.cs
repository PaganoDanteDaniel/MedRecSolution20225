using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.UseCases.Implementations;
using MedRec.Shared.Exceptions.SQLExceptions;
using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.MedicalVisit.UseCases.Tests;

public class CreateMedicalVisitInteractorUoWTests
{
    private static CreateMedicalVisitDto CreateValidDto() =>
        new()
        {
            MedicalHistoryId = Guid.NewGuid(),
            VisitDate = DateTime.UtcNow,
            Reason = "Dolor de cabeza",
            Diagnosis = "Migraña",
            Treatment = "Paracetamol 500mg",
            Notes = "Paciente estable",
            RowVersion = null
        };

    private static (
        Mock<ICreateMedicalVisitOutputPort> outputPort,
        Mock<IMedicalVisitCommandRepositoryUoW> commandRepo,
        Mock<IModelValidatorHub<CreateMedicalVisitDto>> validator,
        Mock<ITemplateFieldDefinitionQueriesRepositoryUoW> templateFieldQueriesRepo,
        Mock<IMedicalVisitDynamicFieldCommandRepositoryUoW> dynamicFieldCommandRepo,
        Mock<IRepositoryUnitOfWork> uow) CreateMocks()
    {
        return (
            new Mock<ICreateMedicalVisitOutputPort>(),
            new Mock<IMedicalVisitCommandRepositoryUoW>(),
            new Mock<IModelValidatorHub<CreateMedicalVisitDto>>(),
            new Mock<ITemplateFieldDefinitionQueriesRepositoryUoW>(),
            new Mock<IMedicalVisitDynamicFieldCommandRepositoryUoW>(),
            new Mock<IRepositoryUnitOfWork>()
        );
    }

    /// <summary>
    /// ExecuteInTransactionWithRetry es responsabilidad del UnitOfWork real (retry, begin/commit/rollback);
    /// para testear el interactor basta con que el mock invoque el delegate recibido.
    /// </summary>
    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task Handle_ShouldReturnValidationErrors_WhenDtoIsInvalid()
    {
        // Arrange
        var dto = new CreateMedicalVisitDto(); // Inválido (campos vacíos)
        var (outputPortMock, commandRepoMock, validatorMock, templateFieldQueriesRepoMock, dynamicFieldCommandRepoMock, uowMock) = CreateMocks();

        validatorMock
            .Setup(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);

        validatorMock
            .SetupGet(v => v.Errors)
            .Returns(new[] { new ValidationError("Reason", "El motivo es obligatorio.") });

        var interactor = new CreateMedicalVisitInteractor(
            outputPortMock.Object,
            commandRepoMock.Object,
            validatorMock.Object,
            templateFieldQueriesRepoMock.Object,
            dynamicFieldCommandRepoMock.Object,
            uowMock.Object
        );

        // Act
        await interactor.Handle(dto, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()), Times.Once);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.Is<IEnumerable<ValidationError>>(e =>
            e != null && e.GetEnumerator().MoveNext())), Times.Once);
        outputPortMock.Verify(x => x.Handle(It.IsAny<PatientMedicalVisit>()), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
        uowMock.Verify(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateMedicalVisit_WhenDtoIsValid()
    {
        // Arrange
        var dto = CreateValidDto();
        var (outputPortMock, commandRepoMock, validatorMock, templateFieldQueriesRepoMock, dynamicFieldCommandRepoMock, uowMock) = CreateMocks();

        validatorMock
            .Setup(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

        SetUpTransactionToRunWork(uowMock);
        uowMock.Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var interactor = new CreateMedicalVisitInteractor(
            outputPortMock.Object,
            commandRepoMock.Object,
            validatorMock.Object,
            templateFieldQueriesRepoMock.Object,
            dynamicFieldCommandRepoMock.Object,
            uowMock.Object
        );

        // Act
        await interactor.Handle(dto, CancellationToken.None);

        // Assert
        commandRepoMock.Verify(c => c.Create(It.IsAny<PatientMedicalVisit>(), It.IsAny<CancellationToken>()), Times.Once);
        dynamicFieldCommandRepoMock.Verify(d => d.CreateRange(It.IsAny<IEnumerable<MedicalVisitDynamicField>>(), It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Once);
        outputPortMock.Verify(x => x.Handle(It.IsAny<PatientMedicalVisit>()), Times.Once);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(null), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateDuplicateKeyException_WithoutSwallowingIt_WhenSaveChangesThrows()
    {
        // Arrange: el interactor ya no atrapa excepciones de infraestructura (ver DIAGNOSTICO_ARQUITECTURA.md,
        // punto 1) — deben propagarse para que el UseCaseExceptionProxy las mapee a ErrorInfo.
        var dto = CreateValidDto();
        var (outputPortMock, commandRepoMock, validatorMock, templateFieldQueriesRepoMock, dynamicFieldCommandRepoMock, uowMock) = CreateMocks();

        validatorMock
            .Setup(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new DuplicateKeyException("Clave duplicada", null, new[] { "PatientMedicalVisit" }));

        var interactor = new CreateMedicalVisitInteractor(
            outputPortMock.Object,
            commandRepoMock.Object,
            validatorMock.Object,
            templateFieldQueriesRepoMock.Object,
            dynamicFieldCommandRepoMock.Object,
            uowMock.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateKeyException>(() => interactor.Handle(dto, CancellationToken.None));

        outputPortMock.Verify(x => x.Handle(It.IsAny<PatientMedicalVisit>()), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPropagateConcurrencyException_WithoutSwallowingIt_WhenSaveChangesThrows()
    {
        // Arrange
        var dto = CreateValidDto();
        var (outputPortMock, commandRepoMock, validatorMock, templateFieldQueriesRepoMock, dynamicFieldCommandRepoMock, uowMock) = CreateMocks();

        validatorMock
            .Setup(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new ConcurrencyException("Concurrencia", null, null));

        var interactor = new CreateMedicalVisitInteractor(
            outputPortMock.Object,
            commandRepoMock.Object,
            validatorMock.Object,
            templateFieldQueriesRepoMock.Object,
            dynamicFieldCommandRepoMock.Object,
            uowMock.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<ConcurrencyException>(() => interactor.Handle(dto, CancellationToken.None));

        outputPortMock.Verify(x => x.Handle(It.IsAny<PatientMedicalVisit>()), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowOperationCanceledException_BeforeStartingTransaction_WhenCanceled()
    {
        // Arrange
        var dto = CreateValidDto();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // simula que la operación fue cancelada

        var (outputPortMock, commandRepoMock, validatorMock, templateFieldQueriesRepoMock, dynamicFieldCommandRepoMock, uowMock) = CreateMocks();

        validatorMock
            .Setup(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

        var interactor = new CreateMedicalVisitInteractor(
            outputPortMock.Object,
            commandRepoMock.Object,
            validatorMock.Object,
            templateFieldQueriesRepoMock.Object,
            dynamicFieldCommandRepoMock.Object,
            uowMock.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => interactor.Handle(dto, cts.Token)
        );

        // La cancelación se detecta antes de abrir la transacción: no debe llegar a tocar el UoW ni el repositorio.
        uowMock.Verify(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
        commandRepoMock.Verify(c => c.Create(It.IsAny<PatientMedicalVisit>(), It.IsAny<CancellationToken>()), Times.Never);
        outputPortMock.Verify(x => x.Handle(It.IsAny<PatientMedicalVisit>()), Times.Never);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
    }
}
