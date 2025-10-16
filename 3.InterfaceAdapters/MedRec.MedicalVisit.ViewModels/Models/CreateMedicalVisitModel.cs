using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.ViewModels.Models;
public class CreateMedicalVisitModel
{
    public Guid PatientId { get; set; }
    public Guid MedicalHistoryId { get; set; }
    public DateTime VisitDate { get; set; } = DateTime.Now;
    public VisitReason Reason { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public int? SystolicPressure { get; set; }
    public int? DiastolicPressure { get; set; }
    public int? PulsePerMinute { get; set; }
    public double? Temperature { get; set; }
    public string Notes { get; set; }

    public static explicit operator CreateMedicalVisitDto(CreateMedicalVisitModel model)
    {
        return new CreateMedicalVisitDto
        {
            MedicalHistoryId = model.MedicalHistoryId,
            VisitDate = model.VisitDate,
            Reason = model.Reason,
            Diagnosis = model.Diagnosis,
            Treatment = model.Treatment,
            SystolicPressure = model.SystolicPressure,
            DiastolicPressure = model.DiastolicPressure,
            PulsePerMinute = model.PulsePerMinute,
            Temperature = model.Temperature,
            Notes = model.Notes
        };
    }
}
