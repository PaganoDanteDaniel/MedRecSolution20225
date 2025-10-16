using MedRec.Entity.Enums;

namespace MedRec.MedicalVisit.BusinessObjects.DTOs;
public class MedicalVisitSummaryDto
{
    public Guid Id { get; init; }
    public DateTime VisitDate { get; init; }
    public VisitReason Reason { get; init; }
    public string Diagnosis { get; init; }
    public string Treatment { get; init; }
    public string Notes { get; init; }

}
