namespace MedRec.MedicalAppointments.BusinessObjects.DTOs;
public class CreateMedicalAppointmentDto
{
    public CreateMedicalAppointmentDto(DateTime dateTime, Guid patientId,
        Guid doctorId, string reason)
    {
        DateTime = dateTime;
        PatientId = patientId;
        ProfessionalId = doctorId;
        Reason = reason;
    }

    public DateTime DateTime { get; init; }
    public Guid PatientId { get; init; }
    public Guid ProfessionalId { get; init; }
    public string Reason { get; init; }
}
