using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
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

    [Fact]
    public async Task Handle_ShouldReturnValidationErrors_WhenDtoIsInvalid()
    {
        // Arrange
        var dto = new CreateMedicalVisitDto(); // Inválido (campos vacíos)

        var outputPortMock = new Mock<ICreateMedicalVisitOutputPort>();
        var commandRepoMock = new Mock<IMedicalVisitCommandRepositoryUoW>();
        var uowMock = new Mock<IRepositoryUnitOfWork>();
        var validatorMock = new Mock<IModelValidatorHub<CreateMedicalVisitDto>>();

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
            uowMock.Object
        );

        // Act
        await interactor.Handle(dto, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()), Times.Once);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.Is<IEnumerable<ValidationError>>(e =>
            e != null && e.GetEnumerator().MoveNext())), Times.Once);
        outputPortMock.Verify(x => x.Handle(), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateMedicalVisit_WhenDtoIsValid()
    {
        // Arrange
        var dto = CreateValidDto();

        var outputPortMock = new Mock<ICreateMedicalVisitOutputPort>();
        var commandRepoMock = new Mock<IMedicalVisitCommandRepositoryUoW>();
        var uowMock = new Mock<IRepositoryUnitOfWork>();
        var validatorMock = new Mock<IModelValidatorHub<CreateMedicalVisitDto>>();

        validatorMock
            .Setup(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

        uowMock.Setup(u => u.BeginTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        uowMock.Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);
        uowMock.Setup(u => u.CommitTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var interactor = new CreateMedicalVisitInteractor(
            outputPortMock.Object,
            commandRepoMock.Object,
            validatorMock.Object,
            uowMock.Object
        );

        // Act
        await interactor.Handle(dto, CancellationToken.None);

        // Assert
        commandRepoMock.Verify(c => c.Create(It.IsAny<PatientMedicalVisit>(), It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.BeginTransaction(It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.CommitTransaction(It.IsAny<CancellationToken>()), Times.Once);
        outputPortMock.Verify(x => x.Handle(), Times.Once);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnDuplicateKeyError_WhenSaveChangesThrowsDuplicateKeyException()
    {
        // Arrange
        var dto = CreateValidDto();

        var outputPortMock = new Mock<ICreateMedicalVisitOutputPort>();
        var commandRepoMock = new Mock<IMedicalVisitCommandRepositoryUoW>();
        var uowMock = new Mock<IRepositoryUnitOfWork>();
        var validatorMock = new Mock<IModelValidatorHub<CreateMedicalVisitDto>>();

        validatorMock
            .Setup(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

        uowMock.Setup(u => u.BeginTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        uowMock.Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new DuplicateKeyException("Clave duplicada", null, new[] { "PatientMedicalVisit" }));
        uowMock.Setup(u => u.RollbackTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var interactor = new CreateMedicalVisitInteractor(
            outputPortMock.Object,
            commandRepoMock.Object,
            validatorMock.Object,
            uowMock.Object
        );

        // Act
        await interactor.Handle(dto, CancellationToken.None);

        // Assert
        uowMock.Verify(u => u.RollbackTransaction(It.IsAny<CancellationToken>()), Times.Once);
        outputPortMock.Verify(x => x.ErrorAsync(It.Is<ErrorInfo>(e =>
            e.Code == ErrorCode.DuplicateKey && e.HttpStatusCode == 409)), Times.Once);
        outputPortMock.Verify(x => x.Handle(), Times.Never);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConcurrencyError_WhenSaveChangesThrowsConcurrencyException()
    {
        // Arrange
        var dto = CreateValidDto();

        var outputPortMock = new Mock<ICreateMedicalVisitOutputPort>();
        var commandRepoMock = new Mock<IMedicalVisitCommandRepositoryUoW>();
        var uowMock = new Mock<IRepositoryUnitOfWork>();
        var validatorMock = new Mock<IModelValidatorHub<CreateMedicalVisitDto>>();

        validatorMock
            .Setup(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

        uowMock.Setup(u => u.BeginTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        uowMock.Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new ConcurrencyException("Concurrencia", null, null));
        uowMock.Setup(u => u.RollbackTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var interactor = new CreateMedicalVisitInteractor(
            outputPortMock.Object,
            commandRepoMock.Object,
            validatorMock.Object,
            uowMock.Object
        );

        // Act
        await interactor.Handle(dto, CancellationToken.None);

        // Assert
        uowMock.Verify(u => u.RollbackTransaction(It.IsAny<CancellationToken>()), Times.Once);
        outputPortMock.Verify(x => x.ErrorAsync(It.Is<ErrorInfo>(e =>
            e.Code == ErrorCode.ConcurrencyError && e.HttpStatusCode == 409)), Times.Once);
        outputPortMock.Verify(x => x.Handle(), Times.Never);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReThrowOperationCanceledException_AfterRollback_WhenCanceled()
    {
        // Arrange
        var dto = CreateValidDto();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // simula que la operación fue cancelada

        var outputPortMock = new Mock<ICreateMedicalVisitOutputPort>();
        var commandRepoMock = new Mock<IMedicalVisitCommandRepositoryUoW>();
        var uowMock = new Mock<IRepositoryUnitOfWork>();
        var validatorMock = new Mock<IModelValidatorHub<CreateMedicalVisitDto>>();

        validatorMock
            .Setup(v => v.Validate(dto, It.IsAny<Func<CreateMedicalVisitDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

        // Seguimiento del orden de ejecución
        bool rollbackCalled = false;

        uowMock.Setup(u => u.BeginTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        uowMock.Setup(u => u.RollbackTransaction(CancellationToken.None))
               .Callback(() => rollbackCalled = true)
               .Returns(Task.CompletedTask);

        var interactor = new CreateMedicalVisitInteractor(
            outputPortMock.Object,
            commandRepoMock.Object,
            validatorMock.Object,
            uowMock.Object
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(
            () => interactor.Handle(dto, cts.Token)
        );

        // Assert
        Assert.True(rollbackCalled, "RollbackTransaction debe haberse llamado antes de relanzar la excepción.");
        uowMock.Verify(u => u.RollbackTransaction(CancellationToken.None), Times.Once);
        outputPortMock.Verify(x => x.Handle(), Times.Never);
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
    }
}
