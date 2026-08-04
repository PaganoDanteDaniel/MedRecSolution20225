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
        cts.ThrowIfCancellationRequested();

        var fields = await _queriesRepository.GetByVisitId(visitId, cts);

        if (!fields.Any())
        {
            await _outputPort.HandleNotFound();
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

        await _outputPort.Handle(dtos);
    }
}
