using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.DynamicTemplates.ViewModels.Orchestration;

namespace MedRec.DynamicTemplates.ViewModels.VM;

/// <summary>
/// ViewModel for getting template field definitions
/// </summary>
public class GetTemplateFieldsVM
{
    private readonly GetTemplateFieldsOrchestrator _orchestrator;

    public GetTemplateFieldsVM(GetTemplateFieldsOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    #region Properties

    public List<TemplateFieldDefinitionModel> Fields { get; set; } = [];
    public bool IsLoading { get; set; }
    public string InformationMessage { get; set; } = string.Empty;

    #endregion

    #region Events

    public event Action? OnFieldsLoaded;
    public event Action? OnShowWarning;
    public event Action? OnShowError;

    #endregion

    #region Methods

    public async Task LoadAsync(Guid specialtyId, CancellationToken cts = default)
    {
        IsLoading = true;
        InformationMessage = string.Empty;
        Fields.Clear();

        try
        {
            var result = await _orchestrator.ExecuteAsync(specialtyId, cts);

            if (result.NotFound)
            {
                InformationMessage = "No hay campos definidos para esta especialidad.";
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
                InformationMessage = result.ErrorMessage ?? "Error al cargar los campos de la plantilla.";
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