using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;

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

            await _outputPort.Handle(dtos);
        }
        catch (OperationCanceledException)
        {
            await _outputPort.ErrorAsync(new ErrorInfo
            {
                Code = ErrorCode.Cancelled,
                Message = "Operación cancelada por el usuario."
            });
        }
        catch (Exception ex)
        {
            await _outputPort.ErrorAsync(new ErrorInfo
            {
                Code = ErrorCode.DatabaseError,
                Message = $"Error al obtener las especialidades activas: {ex.Message}"
            });
        }
    }
}