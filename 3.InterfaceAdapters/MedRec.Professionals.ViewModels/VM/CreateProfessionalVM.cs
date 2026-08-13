using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration.Interfaces;

namespace MedRec.Professionals.ViewModels.VM;
public class CreateProfessionalVM(ICreateProfessionalOrchestrator orchestrator)
{
    public CreateProfessionalModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;
    public bool Success { get; private set; }

    public async Task CreateAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        Success = false;
        try
        {
            InformationMessage = string.Empty;
            var result = await orchestrator.CreateProfessional(Model, ct);

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo crear el profesional.";
            }
            else
            {
                InformationMessage = "Profesional creado correctamente.";
                Success = true;
                Model = new CreateProfessionalModel();
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
