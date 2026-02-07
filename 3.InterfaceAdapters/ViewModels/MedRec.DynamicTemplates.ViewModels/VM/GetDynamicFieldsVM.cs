using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.DynamicTemplates.ViewModels.Orchestration;

namespace MedRec.DynamicTemplates.ViewModels.VM;

/// <summary>
/// ViewModel for getting dynamic field values
/// </summary>
public class GetDynamicFieldsVM
{
    private readonly GetDynamicFieldsOrchestrator _orchestrator;

    public GetDynamicFieldsVM(GetDynamicFieldsOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    #region Properties

    public List<DynamicFieldValueModel> Fields { get; set; } = [];
    public bool IsLoading { get; set; }
    public string InformationMessage { get; set; } = string.Empty;

    #endregion

    #region Events

    public event Action? OnFieldsLoaded;
    public event Action? OnShowWarning;
    public event Action? OnShowError;

    #endregion

    #region Methods

    public async Task LoadAsync(Guid visitId, CancellationToken cts = default)
    {
        IsLoading = true;
        InformationMessage = string.Empty;
        Fields.Clear();

        try
        {
            var result = await _orchestrator.ExecuteAsync(visitId, cts);

            if (result.NotFound)
            {
                InformationMessage = "No se encontraron campos dinámicos para esta visita.";
                OnShowWarning?.Invoke();
                return;
            }

            if (result.Success && result.Fields != null)
            {
                Fields = result.Fields;
                OnFieldsLoaded?.Invoke();
            }
            else
            {
                InformationMessage = result.ErrorMessage ?? "Error al cargar los campos dinámicos.";
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
            IsLoading = false;
        }
    }

    #endregion
}