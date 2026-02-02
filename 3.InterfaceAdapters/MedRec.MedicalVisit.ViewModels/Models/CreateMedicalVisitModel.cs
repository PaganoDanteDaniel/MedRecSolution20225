namespace MedRec.MedicalVisit.ViewModels.Models;
public class CreateMedicalVisitModel : PatientModel
{
    public Guid Id { get; set; }
    public Guid MedicalHistoryId { get; set; }
    public DateTime VisitDate { get; set; } = DateTime.Now;
    public string Reason { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public int? SystolicPressure { get; set; }
    public int? DiastolicPressure { get; set; }
    public int? PulsePerMinute { get; set; }
    public double? Temperature { get; set; }
    public string Notes { get; set; }
    public byte[] RowVersion { get; set; }
    public CreateMedicalVisitModel Clone()
    {
        return new CreateMedicalVisitModel
        {
            MedicalHistoryId = this.MedicalHistoryId,
            Reason = this.Reason?.ToUpperInvariant(),
            Diagnosis = this.Diagnosis?.ToUpperInvariant(),
            Treatment = this.Treatment?.ToUpperInvariant(),
            SystolicPressure = this.SystolicPressure,
            DiastolicPressure = this.DiastolicPressure,
            PulsePerMinute = this.PulsePerMinute,
            Temperature = this.Temperature,
            Notes = this.Notes?.ToUpperInvariant(),
            RowVersion = this.RowVersion
        };
    }
}
