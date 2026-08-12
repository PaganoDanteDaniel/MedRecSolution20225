namespace MedRec.MedicalAppointments.BusinessObjects.DTOs;
public class ReassignMedicalAppointmentDto
{
    public ReassignMedicalAppointmentDto(Guid id,
        DateTime dateTime, Guid patientId,
        Guid doctorId, string reason,
        byte[] rowVersion)
    {
        Id = id;
        DateTime = dateTime;
        PatientId = patientId;
        ProfessionalId = doctorId;
        Reason = reason;
        RowVersion = rowVersion;
    }

    public Guid Id { get; init; }
    public DateTime DateTime { get; init; }
    public Guid PatientId { get; init; }
    public Guid ProfessionalId { get; init; }
    public string Reason { get; init; }
    public byte[] RowVersion { get; init; }
}
