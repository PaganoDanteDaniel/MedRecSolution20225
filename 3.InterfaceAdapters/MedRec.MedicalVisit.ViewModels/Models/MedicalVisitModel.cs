using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.ViewModels.Models;
public class MedicalVisitModel
{
    public Guid Id { get; set; }
    public DateTime VisitDate { get; set; }
    public VisitReason Reason { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Constructor de conveniencia desde DTO
    public MedicalVisitModel(MedicalVisitSummaryDto dto)
    {
        Id = dto.Id;
        VisitDate = dto.VisitDate;
        Reason = dto.Reason;
        Diagnosis = dto.Diagnosis;
        Treatment = dto.Treatment;
        Notes = dto.Notes;
    }
}


