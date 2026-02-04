using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

namespace MedRec.DynamicTemplates.Presenters.Implementation;

/// <summary>
/// Presenter for GetActiveSpecialties use case
/// </summary>
public class GetActiveSpecialtiesPresenter : IGetActiveSpecialtiesOutputPort
{
    public IEnumerable<MedicalSpecialtyDto>? Specialties { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsSuccess { get; private set; }

    /// <inheritdoc/>
    public void Handle(IEnumerable<MedicalSpecialtyDto> specialties)
    {
        Specialties = specialties;
        IsSuccess = true;
        ErrorMessage = null;
    }

    /// <inheritdoc/>
    public void HandleError(string errorMessage)
    {
        ErrorMessage = errorMessage;
        IsSuccess = false;
        Specialties = null;
    }
}