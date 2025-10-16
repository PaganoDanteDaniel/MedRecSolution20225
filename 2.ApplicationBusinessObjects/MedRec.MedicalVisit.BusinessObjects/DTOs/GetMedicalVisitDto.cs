using MedRec.Entity.Enums;
namespace MedRec.MedicalVisit.BusinessObjects.DTOs;
public class GetMedicalVisitDto()
{
    public Guid Id { get; init; }
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
}
