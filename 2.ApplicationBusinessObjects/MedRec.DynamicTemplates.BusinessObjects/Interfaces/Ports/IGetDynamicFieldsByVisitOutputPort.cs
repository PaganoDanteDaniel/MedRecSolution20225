using MedRec.BusinessObjects.Interfaces;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

/// <summary>
/// Output port for GetDynamicFieldsByVisit use case.
/// Inherit from IBaseOutputPort to expose ErrorAsync / ValidationErrorsAsync.
/// </summary>
public interface IGetDynamicFieldsByVisitOutputPort : IBaseOutputPort
{
    Task Handle(IEnumerable<DynamicFieldValueDto> fields);
    Task HandleNotFound();
}