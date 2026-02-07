using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.DynamicTemplates.ViewModels.Orchestration.Interfaces;

namespace MedRec.DynamicTemplates.ViewModels.VM;

/// <summary>
/// ViewModel for saving dynamic field values
/// </summary>
public class SaveDynamicFieldsVM
{
    private readonly ISaveDynamicFieldsOrchestrator _orchestrator;

    public SaveDynamicFieldsVM(ISaveDynamicFieldsOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
        Model = new SaveDynamicFieldsModel();
    }

    #region Properties

    public SaveDynamicFieldsModel Model { get; set; }
    public bool IsSaving { get; set; }
    public string InformationMessage { get; set; } = string.Empty;
    public Dictionary<string, List<string>> ValidationErrors { get; set; } = [];

    #endregion

    #region Events

    public event Action? OnFieldsSaved;
    public event Action? OnValidationError;
    public event Action? OnShowError;

    #endregion

    #region Methods

    public async Task SaveAsync(CancellationToken cts = default)
    {
        IsSaving = true;
        InformationMessage = string.Empty;
        ValidationErrors.Clear();

        try
        {
            var result = await _orchestrator.ExecuteAsync(Model, cts);

            if (result.ValidationErrors != null && result.ValidationErrors.Any())
            {
                ValidationErrors = result.ValidationErrors;
                InformationMessage = "Hay errores de validación en el formulario.";
                OnValidationError?.Invoke();
                return;
            }

            if (result.Success)
            {
                InformationMessage = $"Se guardaron {result.SavedCount} campos correctamente.";
                OnFieldsSaved?.Invoke();
            }
            else
            {
                InformationMessage = result.ErrorMessage ?? "Error al guardar los campos dinámicos.";
                OnShowError?.Invoke();
            }
        }
        catch (Exception ex)
        {
            InformationMessage = $"Error inesperado: {ex.Message}";
            OnShowError?.Invoke();
        }
        finally
        {
            IsSaving = false;
        }
    }

    #endregion
}