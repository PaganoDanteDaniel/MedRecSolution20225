using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;

public class PatientMedicalVisit : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicalHistoryId { get; set; }
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
    public Guid? DoctorId { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

