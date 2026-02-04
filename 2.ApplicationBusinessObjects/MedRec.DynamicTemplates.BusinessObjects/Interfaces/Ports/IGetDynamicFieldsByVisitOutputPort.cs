using MedRec.DynamicTemplates.BusinessObjects.DTOs;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

/// <summary>
/// Output port for GetDynamicFieldsByVisit use case
/// </summary>
public interface IGetDynamicFieldsByVisitOutputPort
{
    /// <summary>
    /// Handles successful retrieval of dynamic field values
    /// </summary>
    /// <param name="fields">Collection of dynamic field values for the visit</param>
    void Handle(IEnumerable<DynamicFieldValueDto> fields);

    /// <summary>
    /// Handles error when retrieving dynamic fields
    /// </summary>
    /// <param name="errorMessage">Error description</param>
    void HandleError(string errorMessage);

    /// <summary>
    /// Handles case when no fields are found for the visit
    /// </summary>
    void HandleNotFound();
}