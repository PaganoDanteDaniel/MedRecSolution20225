using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.UseCases.Implementations;
using MedRec.Shared.Exceptions.SQLExceptions;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.MedicalVisit.UseCases.Tests;

public class GetMedicalHistoryIdInteractorUoWTests
{
    private static (
        Mock<IGetMedicalHistoryIdOutputPort> outputPort,
        Mock<IMedicalVisitQueriesRepositoryUoW> queriesRepo,
        Mock<IMedicalVisitCommandRepositoryUoW> commandRepo,
        Mock<IRepositoryUnitOfWork> uow) CreateMocks()
    {
        return (
            new Mock<IGetMedicalHistoryIdOutputPort>(),
            new Mock<IMedicalVisitQueriesRepositoryUoW>(),
            new Mock<IMedicalVisitCommandRepositoryUoW>(),
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
    public async Task Handle_ShouldReturnValidationError_WhenPatientIdIsEmpty()
    {
        // Arrange: reinstated tras el punto 8 del diagnóstico — el interactor había perdido esta validación
        // al migrar del GetMedicalHistoryIdInteractorUoW.cs original (borrado en el PR #12).
        var (outputPortMock, queriesRepoMock, commandRepoMock, uowMock) = CreateMocks();

        var interactor = new GetMedicalHistoryIdInteractor(
            outputPortMock.Object,
            queriesRepoMock.Object,
            commandRepoMock.Object,
            uowMock.Object
        );

        // Act
        await interactor.Handle(Guid.Empty, CancellationToken.None);

        // Assert
        outputPortMock.Verify(x => x.ValidationErrorsAsync(It.Is<IEnumerable<ValidationError>>(e =>
            e != null && e.GetEnumerator().MoveNext())), Times.Once);
        queriesRepoMock.Verify(x => x.GetMedicalHistory(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        commandRepoMock.Verify(x => x.CreateMedicalHistory(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        outputPortMock.Verify(x => x.Handle(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        uowMock.Verify(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingHistoryId_WhenHistoryExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingHistoryId = Guid.NewGuid();

        var (outputPortMock, queriesRepoMock, commandRepoMock, uowMock) = CreateMocks();

        SetUpTransactionToRunWork(uowMock);
        queriesRepoMock
            .Setup(r => r.GetMedicalHistory(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHistoryId);

        var interactor = new GetMedicalHistoryIdInteractor(
            outputPortMock.Object,
            queriesRepoMock.Object,
            commandRepoMock.Object,
            uowMock.Object
        );

        // Act
        await interactor.Handle(patientId, CancellationToken.None);

        // Assert
        outputPortMock.Verify(x => x.Handle(existingHistoryId, It.IsAny<CancellationToken>()), Times.Once);
        commandRepoMock.Verify(x => x.CreateMedicalHistory(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateHistory_WhenHistoryDoesNotExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var createdHistoryId = Guid.NewGuid();

        var (outputPortMock, queriesRepoMock, commandRepoMock, uowMock) = CreateMocks();

        SetUpTransactionToRunWork(uowMock);
        queriesRepoMock
            .Setup(r => r.GetMedicalHistory(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        commandRepoMock
            .Setup(r => r.CreateMedicalHistory(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdHistoryId);

        uowMock.Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var interactor = new GetMedicalHistoryIdInteractor(
            outputPortMock.Object,
            queriesRepoMock.Object,
            commandRepoMock.Object,
            uowMock.Object
        );

        // Act
        await interactor.Handle(patientId, CancellationToken.None);

        // Assert
        commandRepoMock.Verify(x => x.CreateMedicalHistory(patientId, It.IsAny<CancellationToken>()), Times.Once);
        outputPortMock.Verify(x => x.Handle(createdHistoryId, It.IsAny<CancellationToken>()), Times.Once);
        outputPortMock.Verify(x => x.ErrorAsync(null), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateDuplicateKeyException_WithoutSwallowingIt_WhenSaveChangesThrows()
    {
        // Arrange: el interactor ya no atrapa excepciones de infraestructura ni reintenta la lectura
        // manualmente (esa lógica vivía en el GetMedicalHistoryIdInteractorUoW.cs original, eliminado) —
        // ahora debe propagar para que el UseCaseExceptionProxy la mapee a ErrorInfo.
        var patientId = Guid.NewGuid();

        var (outputPortMock, queriesRepoMock, commandRepoMock, uowMock) = CreateMocks();

        queriesRepoMock
            .Setup(r => r.GetMedicalHistory(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new DuplicateKeyException("Unique constraint violation", null, null));

        var interactor = new GetMedicalHistoryIdInteractor(
            outputPortMock.Object,
            queriesRepoMock.Object,
            commandRepoMock.Object,
            uowMock.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateKeyException>(() => interactor.Handle(patientId, CancellationToken.None));

        outputPortMock.Verify(x => x.Handle(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
    }
}
