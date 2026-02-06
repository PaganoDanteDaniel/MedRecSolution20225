using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.ViewModels.Models;

namespace MedRec.DynamicTemplates.ViewModels.Orchestration;

public class ListSpecialtiesOrchestrator
{
    private readonly IGetActiveSpecialtiesInputPort _inputPort;
    private readonly IGetActiveSpecialtiesOutputPort _outputPort;

    public ListSpecialtiesOrchestrator(
        IGetActiveSpecialtiesInputPort inputPort,
        IGetActiveSpecialtiesOutputPort outputPort)
    {
        _inputPort = inputPort;
        _outputPort = outputPort;
    }

    public async Task<(bool Success, List<MedicalSpecialtyModel>? Specialties, string? ErrorMessage)>
        ExecuteAsync(CancellationToken cts = default)
    {
        await _inputPort.Handle(cts);

        var result = (_outputPort as dynamic).Result as OperationResult<IEnumerable<MedicalSpecialtyDto>>;

        if (result is null)
        {
            return (false, null, "Error al obtener las especialidades activas.");
        }

        if (!result.Success)
        {
            return (false, null, result.Error?.Message ?? "Error al obtener las especialidades activas.");
        }

        var models = result.Value?
            .Select(dto => new MedicalSpecialtyModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Icon = dto.Icon,
                IsActive = dto.IsActive
            })
            .ToList();

        return (true, models, null);
    }
}