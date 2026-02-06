using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;

namespace MedRec.DynamicTemplates.Presenters.Implementation;

/// <summary>
/// Presenter for GetTemplateFieldsBySpecialty use case.
/// </summary>
public class GetTemplateFieldsBySpecialtyPresenter : BaseOutputPort<IEnumerable<TemplateFieldDefinitionDto>>, IGetTemplateFieldsBySpecialtyOutputPort
{
    public Task Handle(IEnumerable<TemplateFieldDefinitionDto> fields)
    {
        Result = OperationResult<IEnumerable<TemplateFieldDefinitionDto>>.Ok(fields);
        return Task.CompletedTask;
    }

    public Task HandleNotFound()
    {
        return ErrorAsync(new ErrorInfo (code: ErrorCode.NotFound, message: "No se encontraron campos de plantilla para la especialidad especificada." ));
    }
}