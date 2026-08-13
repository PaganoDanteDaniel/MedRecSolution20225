using MedRec.BusinessObjects.Results;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
public interface ICreateUserForProfessionalAction
{
    Task<OperationResult<bool>> ExecuteAsync(
        Guid professionalId,
        string email,
        string fullName,
        string temporaryPassword,
        IReadOnlyList<Guid> roleIds,
        CancellationToken ct = default);
}
