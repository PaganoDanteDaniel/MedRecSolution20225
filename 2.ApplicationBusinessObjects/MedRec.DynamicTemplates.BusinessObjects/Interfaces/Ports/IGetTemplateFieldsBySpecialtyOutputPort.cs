using MedRec.DynamicTemplates.BusinessObjects.DTOs;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

/// <summary>
/// Output port for GetTemplateFieldsBySpecialty use case
/// </summary>
public interface IGetTemplateFieldsBySpecialtyOutputPort
{
    /// <summary>
    /// Handles successful retrieval of template field definitions
    /// </summary>
    /// <param name="fields">Collection of template field definitions for the specialty</param>
    void Handle(IEnumerable<TemplateFieldDefinitionDto> fields);

    /// <summary>
    /// Handles error when retrieving template fields
    /// </summary>
    /// <param name="errorMessage">Error description</param>
    void HandleError(string errorMessage);

    /// <summary>
    /// Handles case when no fields are found for the specialty
    /// </summary>
    void HandleNotFound();
}