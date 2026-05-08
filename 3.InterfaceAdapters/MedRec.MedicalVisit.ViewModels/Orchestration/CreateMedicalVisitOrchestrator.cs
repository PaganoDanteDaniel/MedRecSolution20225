using MedRec.BusinessObjects.Results;
using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.MedicalVisit.ViewModels.Orchestration.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration;

internal class CreateMedicalVisitOrchestrator(
    IGetPatientAction getPatientAction,
    IGetMedicalHistoryAction getMedicalHistory,
    IGetTemplateFieldsAction getTemplateFields,
    ICreateMedicalVisitAction createMedicalVisit) : ICreateMedicalVisitOrchestrator
{
    public async Task<OperationResult<Guid>> CreateMedicalVisit(CreateMedicalVisitModel model, CancellationToken ct = default) =>
       await createMedicalVisit.ExecuteAsync(model, ct);

    public async Task<OperationResult<Guid>> GetHistoryId(Guid id, CancellationToken ct) =>
       await getMedicalHistory.ExecuteAsync(id, ct);
    public async Task<OperationResult<CreateMedicalVisitModel>> GetPatient(Guid id, CancellationToken ct)
    {
        var result = await getPatientAction.ExecuteAsync(id, ct);

        if (!result.Success)
        {
            // Propagar error / validaciones recibidas
            return OperationResult.Fail<CreateMedicalVisitModel>(result.Error!, null);
        }

        var patient = result.Value;
        if (patient is null)
        {
            return OperationResult.Unknown<CreateMedicalVisitModel>("Paciente no encontrado.");
        }

        var model = new CreateMedicalVisitModel
        {
            PatientId = patient.PatientId,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            HealthInsuranceName = patient.HealthInsuranceName,
            Acronym = patient.Acronym,
            HealthInsuranceCard = patient.HealthInsuranceCard,
            HealthInsuranceMemberNumber = patient.HealthInsuranceMemberNumber,
            HealthInsurancePlan = patient.HealthInsurancePlan
        };

        return OperationResult.Ok(model);
    }

    public async Task<OperationResult<List<TemplateFieldDefinitionModel>>> GetTemplateFields(Guid specialtyId, CancellationToken cts = default)
    {
        var result = await getTemplateFields.ExecuteAsync(specialtyId, cts);

        if (!result.Success)
        {
            return OperationResult.Fail<List<TemplateFieldDefinitionModel>>(result.Error!, null);
        }

        if (result.Value is null)
        {
            return OperationResult.Unknown<List<TemplateFieldDefinitionModel>>("No se encontraron campos para esta especialidad.");
        }

        var models = result.Value?
            .Select(dto => new TemplateFieldDefinitionModel
            {
                Id = dto.Id,
                SpecialtyId = dto.SpecialtyId,
                FieldName = dto.FieldName,
                FieldLabel = dto.FieldLabel,
                FieldType = dto.FieldType,
                Category = dto.Category,
                IsRequired = dto.IsRequired,
                DisplayOrder = dto.DisplayOrder,
                SelectOptions = dto.SelectOptions,
                DefaultValue = dto.DefaultValue,
                Unit = dto.Unit,
                MinimumValue = dto.MinimumValue,
                MaximumValue = dto.MaximumValue,
                HelpText = dto.HelpText
            })
            .ToList();

        return OperationResult.Ok(models);
    }
}
