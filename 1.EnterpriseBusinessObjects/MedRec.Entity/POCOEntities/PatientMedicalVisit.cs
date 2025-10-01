namespace MedRec.Entity.POCOEntities;
public class PatientMedicalVisit
{
    public Guid Id { get; set; }
    public Guid MedicalHistoryId { get; set; }               // FK a Patient
    public DateTime VisitDate { get; set; }
    public string Reason { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public string Notes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}

