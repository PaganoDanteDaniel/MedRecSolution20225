using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.UseCases.Implementations;
using MedRec.Shared.Exceptions.SQLExceptions;
using Moq;

// Pseudocódigo detallado (plan):
// 1. Caso validación: patientId = Guid.Empty -> interactor debe invocar outputPort.ErrorAsync con ErrorCode.ValidationError y HttpStatusCode = 400.
// 2. Caso historial existente:
//    - Mock GetMedicalHistory devuelve un Guid existente (historyId).
//    - Verificar que NO se llama CreateMedicalHistory.
//    - Verificar que se llama outputPort.Handle(historyId).
// 3. Caso historial inexistente:
//    - Mock GetMedicalHistory devuelve Guid.Empty.
//    - Mock CreateMedicalHistory devuelve nuevo Guid (createdHistoryId).
//    - Verificar que se llama CreateMedicalHistory una vez.
//    - Verificar que se llama outputPort.Handle(createdHistoryId).
// 4. (Opcional) Si hay excepción en repository -> dependiendo de implementación podría llamar ErrorAsync.
//    - No se implementa porque desconocemos manejo interno; se mantiene simple.
// Notas:
// - Convertimos pruebas a métodos async Task para usar await correctamente.
// - Usamos CancellationToken.None.
// - Aislamos dependencias con Moq.
// - Solo verificamos interacciones esperadas (output port y repositorios).

namespace MedRec.MedicalVisit.UseCases.Tests;

public class GetMedicalHistoryIdInteractorUoWTests
{
    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var outputPortMock = new Mock<IGetMedicalHistoryIdOutputPort>();
        var queriesRepoMock = new Mock<IMedicalVisitQueriesRepositoryUoW>();
        var commandRepoMock = new Mock<IMedicalVisitCommandRepositoryUoW>();
        var uowMock = new Mock<IRepositoryUnitOfWork>();

        var interactor = new GetMedicalHistoryIdInteractor(
            outputPortMock.Object,
            queriesRepoMock.Object,
            commandRepoMock.Object,
            uowMock.Object
        );

        // Act
        await interactor.Handle(Guid.Empty, CancellationToken.None);

        // Assert
        outputPortMock.Verify(x => x.ErrorAsync(It.Is<ErrorInfo>(e =>
            e.Code == ErrorCode.ValidationError && e.HttpStatusCode == 400)), Times.Once);

        queriesRepoMock.Verify(x => x.GetMedicalHistory(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        commandRepoMock.Verify(x => x.CreateMedicalHistory(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        outputPortMock.Verify(x => x.Handle(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingHistoryId_WhenHistoryExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingHistoryId = Guid.NewGuid();

        var outputPortMock = new Mock<IGetMedicalHistoryIdOutputPort>();
        var queriesRepoMock = new Mock<IMedicalVisitQueriesRepositoryUoW>();
        var commandRepoMock = new Mock<IMedicalVisitCommandRepositoryUoW>();
        var uowMock = new Mock<IRepositoryUnitOfWork>();

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

        var outputPortMock = new Mock<IGetMedicalHistoryIdOutputPort>();
        var queriesRepoMock = new Mock<IMedicalVisitQueriesRepositoryUoW>();
        var commandRepoMock = new Mock<IMedicalVisitCommandRepositoryUoW>();
        var uowMock = new Mock<IRepositoryUnitOfWork>();

        queriesRepoMock
            .Setup(r => r.GetMedicalHistory(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        commandRepoMock
            .Setup(r => r.CreateMedicalHistory(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdHistoryId);

        // Simular transacción exitosa
        uowMock.Setup(u => u.BeginTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        uowMock.Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);
        uowMock.Setup(u => u.CommitTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

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
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
    }



    [Fact]
    public async Task Handle_ShouldReturnExistingHistoryId_WhenDuplicateKeyOccursOnCreate()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingHistoryId = Guid.NewGuid();

        var outputPortMock = new Mock<IGetMedicalHistoryIdOutputPort>();
        var queriesRepoMock = new Mock<IMedicalVisitQueriesRepositoryUoW>();
        var commandRepoMock = new Mock<IMedicalVisitCommandRepositoryUoW>();
        var uowMock = new Mock<IRepositoryUnitOfWork>();

        // 1. Primera lectura: no existe -> segunda: sí existe
        queriesRepoMock
            .SetupSequence(r => r.GetMedicalHistory(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty)
            .ReturnsAsync(existingHistoryId);

        // 2. CreateMedicalHistory devuelve un ID (no lanza excepción)
        commandRepoMock
            .Setup(r => r.CreateMedicalHistory(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // 3. ¡SaveChanges es donde falla!
        uowMock.Setup(u => u.BeginTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        uowMock.Setup(u => u.SaveChanges(It.IsAny<CancellationToken>()))
               .ThrowsAsync(new DuplicateKeyException("Unique constraint violation", null, null));
        uowMock.Setup(u => u.RollbackTransaction(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

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
        outputPortMock.Verify(x => x.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Never);
        commandRepoMock.Verify(x => x.CreateMedicalHistory(patientId, It.IsAny<CancellationToken>()), Times.Once);
        queriesRepoMock.Verify(x => x.GetMedicalHistory(patientId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}