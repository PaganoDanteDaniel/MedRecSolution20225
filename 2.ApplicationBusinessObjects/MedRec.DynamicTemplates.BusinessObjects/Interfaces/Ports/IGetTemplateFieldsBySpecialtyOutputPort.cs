using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.Entity.POCOEntities;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

/// <summary>
/// Output port for GetTemplateFieldsBySpecialty use case.
/// Inherit from IBaseOutputPort to expose ErrorAsync / ValidationErrorsAsync.
/// </summary>
public interface IGetTemplateFieldsBySpecialtyOutputPort : IBaseOutputPort
{
    OperationResult<IEnumerable<TemplateFieldDefinitionDto>> Result { get; }
    Task Handle(IEnumerable<TemplateFieldDefinition> fields);
    Task HandleNotFound();
}