using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.Entity.Enums;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration;

/// <summary>
/// Orquestador para obtener las definiciones de campos de plantilla por especialidad.
/// - Invoca el InputPort y lee el OperationResult del Presenter.
/// - Diferencia el caso NotFound para que la UI pueda reaccionar correctamente.
/// </summary>
public class GetTemplateFieldsOrchestrator
{
    private readonly IGetTemplateFieldsBySpecialtyInputPort _inputPort;
    private readonly IGetTemplateFieldsBySpecialtyOutputPort _outputPort;

    /// <summary>
    /// Crea una nueva instancia del orquestador.
    /// </summary>
    /// <param name="inputPort">InputPort del caso de uso GetTemplateFieldsBySpecialty</param>
    /// <param name="outputPort">OutputPort del caso de uso GetTemplateFieldsBySpecialty</param>
    public GetTemplateFieldsOrchestrator(
        IGetTemplateFieldsBySpecialtyInputPort inputPort,
        IGetTemplateFieldsBySpecialtyOutputPort outputPort)
    {
        _inputPort = inputPort;
        _outputPort = outputPort;
    }

    /// <summary>
    /// Ejecuta la obtención de campos de plantilla para la especialidad indicada.
    /// </summary>
    /// <param name="specialtyId">Id de la especialidad</param>
    /// <param name="cts">Token de cancelación opcional</param>
    /// <returns>
    /// Tupla con:
    /// - Success: true si la operación fue correcta.
    /// - Fields: lista de modelos de campos (o null).
    /// - ErrorMessage: mensaje en caso de error.
    /// - NotFound: true si no hay campos definidos para la especialidad.
    /// </returns>
    public async Task<(bool Success, List<TemplateFieldDefinitionModel>? Fields, string? ErrorMessage, bool NotFound)>
        ExecuteAsync(Guid specialtyId, CancellationToken cts = default)
    {
        await _inputPort.Handle(specialtyId, cts);

        // Presenter hereda de BaseOutputPort<IEnumerable<TemplateFieldDefinitionDto>>
        var result = (_outputPort as dynamic).Result as OperationResult<IEnumerable<TemplateFieldDefinitionDto>>;

        if (result is null)
        {
            return (false, null, "Error al obtener los campos de plantilla.", false);
        }

        // NotFound se representa por Error con Code = NotFound
        if (result.Error?.Code == ErrorCode.NotFound)
        {
            return (false, null, result.Error.Message, true);
        }

        if (!result.Success)
        {
            return (false, null, result.Error?.Message ?? "Error al obtener los campos de plantilla.", false);
        }

        var models = result.Value?
            .Select(dto => new TemplateFieldDefinitionModel
            {
                Id = dto.Id,
                SpecialtyId = dto.SpecialtyId,
                FieldName = dto.FieldName,
                FieldLabel = dto.FieldLabel,
                FieldType = dto.FieldType,
                Category = dto.Category,
                IsRequired = dto.IsRequired,
                DisplayOrder = dto.DisplayOrder,
                SelectOptions = dto.SelectOptions,
                DefaultValue = dto.DefaultValue,
                Unit = dto.Unit,
                MinimumValue = dto.MinimumValue,
                MaximumValue = dto.MaximumValue,
                HelpText = dto.HelpText
            })
            .ToList();

        return (true, models, null, false);
    }
}