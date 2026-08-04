using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.BusinessObjects.Validator;
using MedRec.Validator.Interfaces;

namespace MedRec.MedicalVisit.UseCases.Implementations;

public class CreateMedicalVisitInteractor(
    ICreateMedicalVisitOutputPort outputPort,
    IMedicalVisitCommandRepositoryUoW commandRepository,
    IModelValidatorHub<CreateMedicalVisitDto> validatorHub,
    ITemplateFieldDefinitionQueriesRepositoryUoW templateFieldQueriesRepository,
    IMedicalVisitDynamicFieldCommandRepositoryUoW dynamicFieldCommandRepository,
    IRepositoryUnitOfWork unitOfWork) : ICreateMedicalVisitInputPort
{
    public async Task Handle(CreateMedicalVisitDto dto, CancellationToken cts = default)
    {
        var fieldDefinitions = await LoadFieldDefinitionsAsync(dto.SpecialtyId, cts);

        if (!await validatorHub.Validate(dto, v => CreateMedicalVisitValidator.Validate(v, fieldDefinitions)))
        {
            await outputPort.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        cts.ThrowIfCancellationRequested();

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            var medicalVisit = (PatientMedicalVisit)dto;

            await commandRepository.Create(medicalVisit, cts);
            await unitOfWork.SaveChanges(cts);

            var dynamicFields = dto.DynamicFields
            .Select(df => new MedicalVisitDynamicField
            {
                PatientMedicalVisitId = medicalVisit.Id,
                FieldDefinitionId = df.FieldDefinitionId,
                FieldValue = df.FieldValue,
                NumericValue = df.NumericValue,
                DateValue = df.DateValue,
                BooleanValue = df.BooleanValue
            });

            await dynamicFieldCommandRepository.CreateRange(dynamicFields, cts);
            await unitOfWork.SaveChanges(cts);

            await outputPort.ErrorAsync(null);
            await outputPort.Handle(medicalVisit);
        }, cts);
    }

    private async Task<IReadOnlyCollection<TemplateFieldDefinitionDto>> LoadFieldDefinitionsAsync(Guid specialtyId, CancellationToken ct)
    {
        if (specialtyId == Guid.Empty)
        {
            return Array.Empty<TemplateFieldDefinitionDto>();
        }

        var entities = await templateFieldQueriesRepository.GetActiveBySpecialtyId(specialtyId, ct);

        return entities
            .Select(MapTemplateFieldDefinitionToDto)
            .ToArray();
    }

    private static TemplateFieldDefinitionDto MapTemplateFieldDefinitionToDto(TemplateFieldDefinition source) => new()
    {
        Id = source.Id,
        SpecialtyId = source.SpecialtyId,
        FieldName = source.FieldName,
        FieldLabel = source.FieldLabel,
        FieldType = source.FieldType,
        Category = source.Category,
        IsRequired = source.IsRequired,
        DisplayOrder = source.DisplayOrder,
        SelectOptions = source.SelectOptions,
        DefaultValue = source.DefaultValue,
        Unit = source.Unit,
        MinimumValue = source.MinimumValue,
        MaximumValue = source.MaximumValue,
        HelpText = source.HelpText
    };
}
