using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.DynamicTemplates.ViewModels.Orchestration.Interfaces;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration;

/// <summary>
/// Orquestador encargado de la operación de guardado de valores de campos dinámicos.
/// - Convierte el modelo de vista a DTO.
/// - Invoca el InputPort de guardado.
/// - Devuelve validaciones o resultado de guardado para que la UI lo gestione.
/// </summary>
public class SaveDynamicFieldsOrchestrator : ISaveDynamicFieldsOrchestrator
{
    private readonly ISaveDynamicFieldsInputPort _inputPort;
    private readonly ISaveDynamicFieldsOutputPort _outputPort;

    public SaveDynamicFieldsOrchestrator(
        ISaveDynamicFieldsInputPort inputPort,
        ISaveDynamicFieldsOutputPort outputPort)
    {
        _inputPort = inputPort;
        _outputPort = outputPort;
    }

    public async Task<(bool Success, int SavedCount, Dictionary<string, List<string>>? ValidationErrors, string? ErrorMessage)>
        ExecuteAsync(SaveDynamicFieldsModel model, CancellationToken cts = default)
    {
        // Conversión explícita Model -> DTO
        var dto = (SaveDynamicFieldsDto)model;

        // Invocación del caso de uso
        await _inputPort.Handle(dto, cts);

        // Presenter hereda de BaseOutputPort<int>
        var result = (_outputPort as dynamic).Result as OperationResult<int>;

        if (result is null)
        {
            return (false, 0, null, "Error al guardar los campos dinámicos.");
        }

        if (result.HasValidationErrors)
        {
            // OperationResult<int> expone ValidationErrors (IReadOnlyList<ValidationError>)
            var validationErrors = result.ValidationErrors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToList()
                );

            return (false, 0, validationErrors, "Hay errores de validación en el formulario.");
        }

        if (!result.Success)
        {
            return (false, 0, null, result.Error?.Message ?? "Error al guardar los campos dinámicos.");
        }

        return (true, result.Value, null, null);
    }
}