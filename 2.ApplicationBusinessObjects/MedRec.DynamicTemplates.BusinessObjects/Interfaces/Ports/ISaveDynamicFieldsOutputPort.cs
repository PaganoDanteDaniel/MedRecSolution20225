namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

/// <summary>
/// Output port for SaveDynamicFields use case
/// </summary>
public interface ISaveDynamicFieldsOutputPort
{
    /// <summary>
    /// Handles successful save of dynamic fields
    /// </summary>
    /// <param name="savedCount">Number of fields saved</param>
    void Handle(int savedCount);

    /// <summary>
    /// Handles validation errors
    /// </summary>
    /// <param name="errors">Dictionary of field names and their validation errors</param>
    void HandleValidationErrors(Dictionary<string, List<string>> errors);

    /// <summary>
    /// Handles error when saving dynamic fields
    /// </summary>
    /// <param name="errorMessage">Error description</param>
    void HandleError(string errorMessage);
}