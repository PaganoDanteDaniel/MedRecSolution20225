using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class CreateProfessionalOrchestratorTests
{
    [Fact]
    public async Task CreateProfessional_ShouldReturnSuccess_WhenNoUserRequested()
    {
        var professionalId = Guid.NewGuid();
        var createProfessional = new Mock<ICreateProfessionalAction>();
        var createUser = new Mock<ICreateUserForProfessionalAction>();
        var deleteProfessional = new Mock<IDeleteProfessionalAction>();

        createProfessional.Setup(a => a.ExecuteAsync(It.IsAny<CreateProfessionalModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(professionalId));

        var orchestrator = new CreateProfessionalOrchestrator(createProfessional.Object, createUser.Object, deleteProfessional.Object);
        var model = new CreateProfessionalModel { CreateUser = null };

        var result = await orchestrator.CreateProfessional(model, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(professionalId, result.Value);
        createUser.Verify(a => a.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateProfessional_ShouldCreateUser_WhenRequested()
    {
        var professionalId = Guid.NewGuid();
        var createProfessional = new Mock<ICreateProfessionalAction>();
        var createUser = new Mock<ICreateUserForProfessionalAction>();
        var deleteProfessional = new Mock<IDeleteProfessionalAction>();

        createProfessional.Setup(a => a.ExecuteAsync(It.IsAny<CreateProfessionalModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(professionalId));
        createUser.Setup(a => a.ExecuteAsync(professionalId, "ana@medrec.local", "Ana García", "Temporal123!", It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(true));

        var orchestrator = new CreateProfessionalOrchestrator(createProfessional.Object, createUser.Object, deleteProfessional.Object);
        var model = new CreateProfessionalModel
        {
            FirstName = "Ana",
            LastName = "García",
            Email = "ana@medrec.local",
            CreateUser = new CreateUserForProfessionalModel { TemporaryPassword = "Temporal123!", RoleIds = new List<Guid>() }
        };

        var result = await orchestrator.CreateProfessional(model, CancellationToken.None);

        Assert.True(result.Success);
        deleteProfessional.Verify(a => a.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateProfessional_ShouldCompensate_WhenUserCreationFails()
    {
        var professionalId = Guid.NewGuid();
        var createProfessional = new Mock<ICreateProfessionalAction>();
        var createUser = new Mock<ICreateUserForProfessionalAction>();
        var deleteProfessional = new Mock<IDeleteProfessionalAction>();

        createProfessional.Setup(a => a.ExecuteAsync(It.IsAny<CreateProfessionalModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(professionalId));
        createUser.Setup(a => a.ExecuteAsync(professionalId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Fail<bool>(new ErrorInfo("Ya existe un usuario con ese email."), null));
        deleteProfessional.Setup(a => a.ExecuteAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(true));

        var orchestrator = new CreateProfessionalOrchestrator(createProfessional.Object, createUser.Object, deleteProfessional.Object);
        var model = new CreateProfessionalModel
        {
            CreateUser = new CreateUserForProfessionalModel { TemporaryPassword = "Temporal123!", RoleIds = new List<Guid>() }
        };

        var result = await orchestrator.CreateProfessional(model, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Ya existe un usuario con ese email.", result.Error?.Message);
        deleteProfessional.Verify(a => a.ExecuteAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
