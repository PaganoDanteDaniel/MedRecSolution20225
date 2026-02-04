using MedRec.DynamicTemplates.BusinessObjects.DTOs;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

/// <summary>
/// Output port for GetActiveSpecialties use case
/// </summary>
public interface IGetActiveSpecialtiesOutputPort
{
    /// <summary>
    /// Handles successful retrieval of active specialties
    /// </summary>
    /// <param name="specialties">Collection of active medical specialties</param>
    void Handle(IEnumerable<MedicalSpecialtyDto> specialties);

    /// <summary>
    /// Handles error when retrieving active specialties
    /// </summary>
    /// <param name="errorMessage">Error description</param>
    void HandleError(string errorMessage);
}