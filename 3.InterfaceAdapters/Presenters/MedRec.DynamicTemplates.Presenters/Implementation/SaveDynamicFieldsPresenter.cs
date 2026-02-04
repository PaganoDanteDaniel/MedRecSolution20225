using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

namespace MedRec.DynamicTemplates.Presenters.Implementation;

/// <summary>
/// Presenter for SaveDynamicFields use case
/// </summary>
public class SaveDynamicFieldsPresenter : ISaveDynamicFieldsOutputPort
{
    public int SavedCount { get; private set; }
    public Dictionary<string, List<string>>? ValidationErrors { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsSuccess { get; private set; }
    public bool HasValidationErrors { get; private set; }

    /// <inheritdoc/>
    public void Handle(int savedCount)
    {
        SavedCount = savedCount;
        IsSuccess = true;
        HasValidationErrors = false;
        ValidationErrors = null;
        ErrorMessage = null;
    }

    /// <inheritdoc/>
    public void HandleValidationErrors(Dictionary<string, List<string>> errors)
    {
        ValidationErrors = errors;
        HasValidationErrors = true;
        IsSuccess = false;
        SavedCount = 0;
        ErrorMessage = "Se encontraron errores de validación en los campos dinámicos.";
    }

    /// <inheritdoc/>
    public void HandleError(string errorMessage)
    {
        ErrorMessage = errorMessage;
        IsSuccess = false;
        HasValidationErrors = false;
        ValidationErrors = null;
        SavedCount = 0;
    }
}