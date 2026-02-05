using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.Presenters.Implementation;
using MedRec.DynamicTemplates.ViewModels.Models;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration;

/// <summary>
/// Orchestrator for listing active medical specialties
/// </summary>
public class ListSpecialtiesOrchestrator
{
    private readonly IGetActiveSpecialtiesInputPort _inputPort;

    public ListSpecialtiesOrchestrator(IGetActiveSpecialtiesInputPort inputPort)
    {
        _inputPort = inputPort;
    }

    public async Task<(bool Success, List<MedicalSpecialtyModel>? Specialties, string? ErrorMessage)>
        ExecuteAsync(CancellationToken cts = default)
    {
        var presenter = new GetActiveSpecialtiesPresenter();

        await _inputPort.Handle(cts);

        if (!presenter.IsSuccess)
        {
            return (false, null, presenter.ErrorMessage);
        }

        var models = presenter.Specialties?.Select(dto => new MedicalSpecialtyModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            IsActive = dto.IsActive
        }).ToList();

        return (true, models, null);
    }
}