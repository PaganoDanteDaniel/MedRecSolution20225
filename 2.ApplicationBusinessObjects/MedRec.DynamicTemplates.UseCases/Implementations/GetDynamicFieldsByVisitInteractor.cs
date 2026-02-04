using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;

namespace MedRec.DynamicTemplates.UseCases.Implementations;

/// <summary>
/// Interactor for GetDynamicFieldsByVisit use case
/// </summary>
internal class GetDynamicFieldsByVisitInteractor : IGetDynamicFieldsByVisitInputPort
{
    private readonly IGetDynamicFieldsByVisitOutputPort _outputPort;
    private readonly IMedicalVisitDynamicFieldQueriesRepositoryUoW _queriesRepository;

    public GetDynamicFieldsByVisitInteractor(
        IGetDynamicFieldsByVisitOutputPort outputPort,
        IMedicalVisitDynamicFieldQueriesRepositoryUoW queriesRepository)
    {
        _outputPort = outputPort;
        _queriesRepository = queriesRepository;
    }

    public async Task Handle(Guid visitId, CancellationToken cts = default)
    {
        try
        {
            cts.ThrowIfCancellationRequested();

            var fields = await _queriesRepository.GetByVisitId(visitId, cts);

            if (!fields.Any())
            {
                _outputPort.HandleNotFound();
                return;
            }

            var dtos = fields.Select(f => new DynamicFieldValueDto
            {
                FieldDefinitionId = f.FieldDefinitionId,
                FieldValue = f.FieldValue,
                NumericValue = f.NumericValue,
                DateValue = f.DateValue,
                BooleanValue = f.BooleanValue
            }).ToList();

            _outputPort.Handle(dtos);
        }
        catch (OperationCanceledException)
        {
            _outputPort.HandleError("Operación cancelada por el usuario.");
        }
        catch (Exception ex)
        {
            _outputPort.HandleError($"Error al obtener los campos dinámicos de la visita: {ex.Message}");
        }
    }
}