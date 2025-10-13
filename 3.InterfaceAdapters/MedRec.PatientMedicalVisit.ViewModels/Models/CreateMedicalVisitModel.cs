using MedRec.PatientMedicalVisit.BusinessObjects.DTOs;

namespace MedRec.PatientMedicalVisit.ViewModels.Models;
public class CreateMedicalVisitModel
{
    private Guid _patientId;
    private DateTime _visitDate = DateTime.Now;
    private string _visitReason;
    private string _visitDiagnosis;
    private string _visitTreatment;
    private int? _systolicPressure;
    private int? _diastolicPressure;
    private int? _pulsePerMinute;
    private double? _temperatura;
    private string _visitNotes;

    public Guid PatientId { get => _patientId; set => _patientId = value; }
    public DateTime VisitDate { get => _visitDate; set => _visitDate = value; }
    public string VisitReason { get => _visitReason; set => _visitReason = value; }
    public string VisitDiagnosis { get => _visitDiagnosis; set => _visitDiagnosis = value; }
    public string VisitTreatment { get => _visitTreatment; set => _visitTreatment = value; }
    public int? SystolicPressure { get => _systolicPressure; set => _systolicPressure = value; }
    public int? DiastolicPressure { get => _diastolicPressure; set => _diastolicPressure = value; }
    public int? PulsePerMinute { get => _pulsePerMinute; set => _pulsePerMinute = value; }
    public double? Temperature { get => _temperatura; set => _temperatura = value; }
    public string VisitNotes { get => _visitNotes; set => _visitNotes = value; }

    public static explicit operator CreateMedicalVisitDto(CreateMedicalVisitModel model)
    {
        return new CreateMedicalVisitDto
        {
            MedicalHistoryId = model.PatientId,
            VisitDate = model.VisitDate,
            Reason = model.VisitReason,
            Diagnosis = model.VisitDiagnosis,
            Treatment = model.VisitTreatment,
            SystolicPressure = model.SystolicPressure,
            DiastolicPressure = model.DiastolicPressure,
            PulsePerMinute = model.PulsePerMinute,
            Temperature = model.Temperature,
            Notes = model.VisitNotes
        };
    }
}
