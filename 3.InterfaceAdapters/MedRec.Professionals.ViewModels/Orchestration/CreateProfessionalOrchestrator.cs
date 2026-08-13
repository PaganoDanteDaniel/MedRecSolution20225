using MedRec.BusinessObjects.Results;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.Professionals.ViewModels.Orchestration.Interfaces;

namespace MedRec.Professionals.ViewModels.Orchestration;

public class CreateProfessionalOrchestrator(
    ICreateProfessionalAction createProfessional,
    ICreateUserForProfessionalAction createUser,
    IDeleteProfessionalAction deleteProfessional) : ICreateProfessionalOrchestrator
{
    public async Task<OperationResult<Guid>> CreateProfessional(CreateProfessionalModel model, CancellationToken ct = default)
    {
        var professionalResult = await createProfessional.ExecuteAsync(model, ct);
        if (!professionalResult.Success)
            return professionalResult;

        if (model.CreateUser is null)
            return professionalResult;

        var userResult = await createUser.ExecuteAsync(
            professionalResult.Value,
            model.Email,
            $"{model.FirstName} {model.LastName}",
            model.CreateUser.TemporaryPassword,
            model.CreateUser.RoleIds,
            ct);

        if (userResult.Success)
            return professionalResult;

        // Compensación best-effort: si el borrado también falla (p.ej. el usuario actual
        // tiene Professionals_Create pero no Professionals_Delete), igual se propaga el
        // error real de la creación del usuario en vez de uno de permisos que lo taparía.
        await deleteProfessional.ExecuteAsync(professionalResult.Value, ct);

        return OperationResult.Fail<Guid>(
            userResult.Error ?? new MedRec.Entity.DTOs.ErrorInfo("No se pudo crear el usuario del profesional."),
            userResult.MessageAction,
            userResult.ValidationErrors);
    }
}
