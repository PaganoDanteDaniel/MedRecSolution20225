using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;

namespace MedRec.DynamicTemplates.UseCases.Implementations;

/// <summary>
/// Interactor for GetActiveSpecialties use case
/// </summary>
internal class GetActiveSpecialtiesInteractor : IGetActiveSpecialtiesInputPort
{
    private readonly IGetActiveSpecialtiesOutputPort _outputPort;
    private readonly IMedicalSpecialtyQueriesRepositoryUoW _queriesRepository;

    public GetActiveSpecialtiesInteractor(
        IGetActiveSpecialtiesOutputPort outputPort,
        IMedicalSpecialtyQueriesRepositoryUoW queriesRepository)
    {
        _outputPort = outputPort;
        _queriesRepository = queriesRepository;
    }

    public async Task Handle(CancellationToken cts = default)
    {
        try
        {
            cts.ThrowIfCancellationRequested();

            var specialties = await _queriesRepository.GetActiveSpecialties(cts);

            var dtos = specialties.Select(s => new MedicalSpecialtyDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Icon = s.Icon,
                IsActive = s.IsActive
            }).ToList();

            _outputPort.Handle(dtos);
        }
        catch (OperationCanceledException)
        {
            _outputPort.HandleError("Operación cancelada por el usuario.");
        }
        catch (Exception ex)
        {
            _outputPort.HandleError($"Error al obtener las especialidades activas: {ex.Message}");
        }
    }
}