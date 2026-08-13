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
            return userResult.Warning is null
                ? professionalResult
                : OperationResult.Ok(professionalResult.Value, userResult.Warning);

        // Compensación best-effort: si el borrado también falla (p.ej. el usuario actual
        // tiene Professionals_Create pero no Professionals_Delete), igual se propaga el
        // error real de la creación del usuario en vez de uno de permisos que lo taparía.
        var compensationResult = await deleteProfessional.ExecuteAsync(professionalResult.Value, ct);

        var baseError = userResult.Error ?? new MedRec.Entity.DTOs.ErrorInfo("No se pudo crear el usuario del profesional.");
        var finalError = compensationResult.Success
            ? baseError
            : new MedRec.Entity.DTOs.ErrorInfo(
                baseError.Message + " El profesional quedó creado sin usuario asociado; revisá el listado.",
                baseError.Code,
                baseError.Details,
                baseError.HttpStatusCode);

        return OperationResult.Fail<Guid>(
            finalError,
            userResult.MessageAction,
            userResult.ValidationErrors);
    }
}
