using MedRec.BusinessObjects.Results;
using MedRec.Professionals.ViewModels.Models;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
public interface ICreateProfessionalAction
{
    Task<OperationResult<Guid>> ExecuteAsync(CreateProfessionalModel model, CancellationToken ct = default);
}
