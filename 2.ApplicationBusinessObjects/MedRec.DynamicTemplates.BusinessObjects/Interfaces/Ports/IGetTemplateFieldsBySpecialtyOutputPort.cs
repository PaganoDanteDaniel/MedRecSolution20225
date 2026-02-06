using MedRec.BusinessObjects.Interfaces;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

/// <summary>
/// Output port for GetTemplateFieldsBySpecialty use case.
/// Inherit from IBaseOutputPort to expose ErrorAsync / ValidationErrorsAsync.
/// </summary>
public interface IGetTemplateFieldsBySpecialtyOutputPort : IBaseOutputPort
{
    Task Handle(IEnumerable<TemplateFieldDefinitionDto> fields);
    Task HandleNotFound();
}