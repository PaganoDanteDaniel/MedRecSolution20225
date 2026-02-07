using MedRec.DynamicTemplates.ViewModels.Models;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration.Interfaces;

public interface IListSpecialtiesOrchestrator
{
    Task<(bool Success, List<MedicalSpecialtyModel>? Specialties, string? ErrorMessage)>
        ExecuteAsync(CancellationToken cts = default);
}