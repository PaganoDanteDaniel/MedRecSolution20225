using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
namespace MedRec.MedicalVisit.BusinessObjects.DTOs;
public class CreateMedicalVisitDto()
{
    public Guid MedicalHistoryId { get; init; }
    public DateTime VisitDate { get; init; }
    public VisitReason Reason { get; init; }
    public string Diagnosis { get; init; }
    public string Treatment { get; init; }
    public int? SystolicPressure { get; init; }
    public int? DiastolicPressure { get; init; }
    public int? PulsePerMinute { get; init; }
    public double? Temperature { get; init; }
    public string Notes { get; init; }
    //public IEnumerable<CreatePatientLaboratoryResultDto> PatientLaboratoryResults { get; init; } = [];

    public static explicit operator PatientMedicalVisit(CreateMedicalVisitDto dto) =>
        new()
        {
            MedicalHistoryId = dto.MedicalHistoryId,
            VisitDate = dto.VisitDate,
            Reason = dto.Reason,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            SystolicPressure = dto.SystolicPressure,
            DiastolicPressure = dto.DiastolicPressure,
            PulsePerMinute = dto.PulsePerMinute,
            Temperature = dto.Temperature,
            Notes = dto.Notes
        };
}
