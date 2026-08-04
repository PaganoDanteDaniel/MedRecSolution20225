using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;

namespace MedRec.DynamicTemplates.UseCases.Implementations;

/// <summary>
/// Interactor for GetTemplateFieldsBySpecialty use case
/// </summary>
internal class GetTemplateFieldsBySpecialtyInteractor : IGetTemplateFieldsBySpecialtyInputPort
{
    private readonly IGetTemplateFieldsBySpecialtyOutputPort _outputPort;
    private readonly ITemplateFieldDefinitionQueriesRepositoryUoW _queriesRepository;

    public GetTemplateFieldsBySpecialtyInteractor(
        IGetTemplateFieldsBySpecialtyOutputPort outputPort,
        ITemplateFieldDefinitionQueriesRepositoryUoW queriesRepository)
    {
        _outputPort = outputPort;
        _queriesRepository = queriesRepository;
    }

    public async Task Handle(Guid specialtyId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        var fields = await _queriesRepository.GetBySpecialtyId(specialtyId, cts);

        if (!fields.Any())
        {
            await _outputPort.HandleNotFound();
            return;
        }

        await _outputPort.Handle(fields);
    }
}
