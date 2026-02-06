using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.Entity.Enums;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration;

/// <summary>
/// Orquestador para obtener los valores de campos dinámicos asociados a una visita médica.
/// - Usa el OperationResult del Presenter para diferenciar NotFound y errores generales.
/// - Mapea DTOs a modelos de vista para su consumo en la UI.
/// </summary>
public class GetDynamicFieldsOrchestrator
{
    private readonly IGetDynamicFieldsByVisitInputPort _inputPort;
    private readonly IGetDynamicFieldsByVisitOutputPort _outputPort;

    public GetDynamicFieldsOrchestrator(
        IGetDynamicFieldsByVisitInputPort inputPort,
        IGetDynamicFieldsByVisitOutputPort outputPort)
    {
        _inputPort = inputPort;
        _outputPort = outputPort;
    }

    public async Task<(bool Success, List<DynamicFieldValueModel>? Fields, string? ErrorMessage, bool NotFound)>
        ExecuteAsync(Guid visitId, CancellationToken cts = default)
    {
        await _inputPort.Handle(visitId, cts);

        // Presenter hereda de BaseOutputPort<IEnumerable<DynamicFieldValueDto>>
        var result = (_outputPort as dynamic).Result as OperationResult<IEnumerable<DynamicFieldValueDto>>;

        if (result is null)
        {
            return (false, null, "Error al obtener los campos dinámicos de la visita.", false);
        }

        if (result.Error?.Code == ErrorCode.NotFound)
        {
            return (false, null, result.Error.Message, true);
        }

        if (!result.Success)
        {
            return (false, null, result.Error?.Message ?? "Error al obtener los campos dinámicos de la visita.", false);
        }

        var models = result.Value?
            .Select(dto => new DynamicFieldValueModel
            {
                FieldDefinitionId = dto.FieldDefinitionId,
                FieldValue = dto.FieldValue,
                NumericValue = dto.NumericValue,
                DateValue = dto.DateValue,
                BooleanValue = dto.BooleanValue
            })
            .ToList();

        return (true, models, null, false);
    }
}