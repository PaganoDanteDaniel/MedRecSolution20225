using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.ViewModels.Orchestration;
public static class MedicalVisitMapper
{
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

    public static CreateMedicalVisitDto ToCreateDto(CreateMedicalVisitModel model) => new()
    {
        MedicalHistoryId = model.MedicalHistoryId,
        VisitDate = model.VisitDate,
        Reason = (model.Reason ?? string.Empty).ToUpperInvariant(),
        Diagnosis = (model.Diagnosis ?? string.Empty).ToUpperInvariant(),
        Treatment = (model.Treatment ?? string.Empty).ToUpperInvariant(),
        SystolicPressure = model.SystolicPressure,
        DiastolicPressure = model.DiastolicPressure,
        PulsePerMinute = model.PulsePerMinute,
        Temperature = model.Temperature,
        Notes = (model.Notes ?? string.Empty).ToUpperInvariant(),
        RowVersion = model.RowVersion
    };
}

