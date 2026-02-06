using MedRec.BusinessObjects.Interfaces;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

/// <summary>
/// Output port for GetActiveSpecialties use case.
/// Inherit from IBaseOutputPort to expose ErrorAsync / ValidationErrorsAsync.
/// </summary>
public interface IGetActiveSpecialtiesOutputPort : IBaseOutputPort
{
    Task Handle(IEnumerable<MedicalSpecialtyDto> specialties);
}