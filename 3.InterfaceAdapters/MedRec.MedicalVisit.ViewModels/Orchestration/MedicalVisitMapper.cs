using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.ViewModels.Models;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.ViewModels.Orchestration;

public static class MedicalVisitMapper
{
    public static PatientModel ToPatientModel(PatientForMedicalVisitDto dto) => new()
    {
        FullName = dto.FullName,
        DateOfBirth = dto.DateOfBirth,
        HealthInsuranceName = dto.HealthInsuranceName,
        Acronym = dto.Acronym,
        HealthInsuranceMemberNumber = dto.HealthInsuranceMemberNumber,
        HealthInsuranceCard = dto.HealthInsuranceCard,
        HealthInsurancePlan = dto.HealthInsurancePlan
    };
    public static CreateMedicalVisitModel ToCreateModel(PatientForMedicalVisitDto dto) => new()
    {
        FullName = dto.FullName,
        DateOfBirth = dto.DateOfBirth,
        HealthInsuranceName = dto.HealthInsuranceName,
        Acronym = dto.Acronym,
        HealthInsuranceCard = dto.HealthInsuranceCard,
        HealthInsuranceMemberNumber = dto.HealthInsuranceMemberNumber,
        HealthInsurancePlan = dto.HealthInsurancePlan
    };
    public static UpdateMedicalVisitModel ToUpdateModel(GetMedicalVisitDto dto) => new()
    {
        Id = dto.Id,
        MedicalHistoryId = dto.MedicalHistoryId,
        VisitDate = dto.VisitDate,
        Reason = dto.Reason,
        Diagnosis = (dto.Diagnosis ?? string.Empty).ToUpperInvariant(),
        Treatment = (dto.Treatment ?? string.Empty).ToUpperInvariant(),
        SystolicPressure = dto.SystolicPressure,
        DiastolicPressure = dto.DiastolicPressure,
        PulsePerMinute = dto.PulsePerMinute,
        Temperature = dto.Temperature,
        Notes = (dto.Notes ?? string.Empty).ToUpperInvariant(),
        RowVersion = dto.RowVersion
    };
    public static CreateMedicalVisitDto ToCreateDto(CreateMedicalVisitModel model) => new()
    {
        MedicalHistoryId = model.MedicalHistoryId,
        SpecialtyId = model.SpecialtyId,
        VisitDate = model.VisitDate,
        Reason = (model.Reason ?? string.Empty).ToUpperInvariant(),
        Diagnosis = (model.Diagnosis ?? string.Empty).ToUpperInvariant(),
        Treatment = (model.Treatment ?? string.Empty).ToUpperInvariant(),
        Notes = (model.Notes ?? string.Empty).ToUpperInvariant(),
        DynamicFields = model.DynamicFields?.Select(df => new DynamicFieldValueDto
        {
            FieldDefinitionId = df.FieldDefinitionId,
            FieldValue = df.FieldValue,
            NumericValue = df.NumericValue,
            DateValue = df.DateValue,
            BooleanValue = df.BooleanValue

        }).ToList() ?? new List<DynamicFieldValueDto>(),
        RowVersion = model.RowVersion
    };
    public static UpdateMedicalVisitDto ToUpdateDto(UpdateMedicalVisitModel model) => new()
    {
        Id = model.Id,
        MedicalHistoryId = model.MedicalHistoryId,
        VisitDate = model.VisitDate,
        Reason = (model.Reason ?? string.Empty).ToUpperInvariant(),
        Diagnosis = (model.Diagnosis ?? string.Empty).ToUpperInvariant(),
        Treatment = (model.Treatment ?? string.Empty).ToUpperInvariant(),
        //SystolicPressure = model.SystolicPressure,
        //DiastolicPressure = model.DiastolicPressure,
        //PulsePerMinute = model.PulsePerMinute,
        //Temperature = model.Temperature,
        Notes = (model.Notes ?? string.Empty).ToUpperInvariant(),
        RowVersion = model.RowVersion
    };
    public static TemplateFieldDefinitionDto ToDefinitionDto(TemplateFieldDefinitionModel definition) => new()
    {
        Id = definition.Id,
        SpecialtyId = definition.SpecialtyId,
        FieldName = definition.FieldName,
        FieldLabel = definition.FieldLabel,
        FieldType = definition.FieldType,
        Category = definition.Category,
        IsRequired = definition.IsRequired,
        DisplayOrder = definition.DisplayOrder,
        SelectOptions = definition.SelectOptions,
        DefaultValue = definition.DefaultValue,
        Unit = definition.Unit,
        MinimumValue = definition.MinimumValue,
        MaximumValue = definition.MaximumValue,
        HelpText = definition.HelpText
    };

    public static IReadOnlyList<TemplateFieldDefinitionDto> ToDefinitionDtos(
    IEnumerable<TemplateFieldDefinitionModel> definitions)
    {
        if (definitions is null)
            return Array.Empty<TemplateFieldDefinitionDto>();

        return definitions
            .Select(ToDefinitionDto)
            .ToList();
    }

}

