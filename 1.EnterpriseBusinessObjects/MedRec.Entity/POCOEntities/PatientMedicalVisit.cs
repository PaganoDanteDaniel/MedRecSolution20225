namespace MedRec.Entity.POCOEntities;
public class PatientMedicalVisit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicalHistoryId { get; set; }               // FK a Patient
    public DateTime VisitDate { get; set; }
    public string Reason { get; set; }
    public string Diagnosis { get; set; } = String.Empty;
    public string Treatment { get; set; } = String.Empty;
    public int? SystolicPressure { get; set; }
    public int? DiastolicPressure { get; set; }
    public int? PulsePerMinute { get; set; }
    public double? Temperature { get; set; }
    public string Notes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }

}

