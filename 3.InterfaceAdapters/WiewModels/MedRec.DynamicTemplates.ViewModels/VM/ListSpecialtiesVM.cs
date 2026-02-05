using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.DynamicTemplates.ViewModels.Orchestration;

namespace MedRec.DynamicTemplates.ViewModels.VM;

/// <summary>   
/// ViewModel for listing active medical specialties
/// </summary>
public class ListSpecialtiesVM
{
    private readonly ListSpecialtiesOrchestrator _orchestrator;

    public ListSpecialtiesVM(ListSpecialtiesOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    #region Properties

    public List<MedicalSpecialtyModel> Specialties { get; set; } = [];
    public bool IsLoading { get; set; }
    public string InformationMessage { get; set; } = string.Empty;

    #endregion

    #region Events

    public event Action? OnSpecialtiesLoaded;
    public event Action? OnShowError;

    #endregion

    #region Methods

    public async Task LoadAsync(CancellationToken cts = default)
    {
        IsLoading = true;
        InformationMessage = string.Empty;

        try
        {
            var result = await _orchestrator.ExecuteAsync(cts);

            if (result.Success && result.Specialties != null)
            {
                Specialties = result.Specialties;
                OnSpecialtiesLoaded?.Invoke();
            }
            else
            {
                InformationMessage = result.ErrorMessage ?? "Error al cargar las especialidades.";
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