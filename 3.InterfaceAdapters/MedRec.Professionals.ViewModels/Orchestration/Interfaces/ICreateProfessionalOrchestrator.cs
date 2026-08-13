using MedRec.BusinessObjects.Results;
using MedRec.Professionals.ViewModels.Models;

namespace MedRec.Professionals.ViewModels.Orchestration.Interfaces;
public interface ICreateProfessionalOrchestrator
{
    Task<OperationResult<Guid>> CreateProfessional(CreateProfessionalModel model, CancellationToken ct = default);
}
