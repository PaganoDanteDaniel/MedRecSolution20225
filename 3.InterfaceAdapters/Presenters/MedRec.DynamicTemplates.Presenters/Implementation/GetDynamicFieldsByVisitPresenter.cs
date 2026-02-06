using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;

namespace MedRec.DynamicTemplates.Presenters.Implementation;

/// <summary>
/// Presenter for GetDynamicFieldsByVisit use case.
/// </summary>
public class GetDynamicFieldsByVisitPresenter : BaseOutputPort<IEnumerable<DynamicFieldValueDto>>, IGetDynamicFieldsByVisitOutputPort
{
    public Task Handle(IEnumerable<DynamicFieldValueDto> fields)
    {
        Result = OperationResult<IEnumerable<DynamicFieldValueDto>>.Ok(fields);
        return Task.CompletedTask;
    }

    public Task HandleNotFound()
    {
        return ErrorAsync(new ErrorInfo(code: ErrorCode.NotFound, message: "No se encontraron campos dinámicos para la visita médica especificada."));
    }
}