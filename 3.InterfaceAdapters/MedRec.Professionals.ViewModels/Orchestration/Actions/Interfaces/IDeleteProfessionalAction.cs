using MedRec.BusinessObjects.Results;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
public interface IDeleteProfessionalAction
{
    Task<OperationResult<bool>> ExecuteAsync(Guid professionalId, CancellationToken ct = default);
}
