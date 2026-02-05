using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.Presenters.Implementation;
using MedRec.DynamicTemplates.ViewModels.Models;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration;

/// <summary>
/// Orchestrator for saving dynamic field values
/// </summary>
public class SaveDynamicFieldsOrchestrator
{
    private readonly ISaveDynamicFieldsInputPort _inputPort;

    public SaveDynamicFieldsOrchestrator(ISaveDynamicFieldsInputPort inputPort)
    {
        _inputPort = inputPort;
    }

    public async Task<(bool Success, int SavedCount, Dictionary<string, List<string>>? ValidationErrors, string? ErrorMessage)>
        ExecuteAsync(SaveDynamicFieldsModel model, CancellationToken cts = default)
    {
        var presenter = new SaveDynamicFieldsPresenter();

        var dto = (SaveDynamicFieldsDto)model;
        await _inputPort.Handle(dto, cts);

        if (presenter.HasValidationErrors)
        {
            return (false, 0, presenter.ValidationErrors, presenter.ErrorMessage);
        }

        if (!presenter.IsSuccess)
        {
            return (false, 0, null, presenter.ErrorMessage);
        }

        return (true, presenter.SavedCount, null, null);
    }
}