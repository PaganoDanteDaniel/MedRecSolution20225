using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;

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
        try
        {
            cts.ThrowIfCancellationRequested();

            var fields = await _queriesRepository.GetBySpecialtyId(specialtyId, cts);

            if (!fields.Any())
            {
                await _outputPort.HandleNotFound();
                return;
            }

            var dtos = fields.Select(f => new TemplateFieldDefinitionDto
            {
                Id = f.Id,
                SpecialtyId = f.SpecialtyId,
                FieldName = f.FieldName,
                FieldLabel = f.FieldLabel,
                FieldType = f.FieldType,
                Category = f.Category,
                IsRequired = f.IsRequired,
                DisplayOrder = f.DisplayOrder,
                SelectOptions = f.SelectOptions,
                DefaultValue = f.DefaultValue,
                Unit = f.Unit,
                MinimumValue = f.MinimumValue,
                MaximumValue = f.MaximumValue,
                HelpText = f.HelpText
            }).OrderBy(f => f.DisplayOrder).ToList();

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
                Message = $"Error al obtener los campos de plantilla: {ex.Message}"
            });
        }
    }
}