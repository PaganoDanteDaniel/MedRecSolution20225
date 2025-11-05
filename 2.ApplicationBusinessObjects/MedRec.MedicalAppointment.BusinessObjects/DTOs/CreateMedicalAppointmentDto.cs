namespace MedRec.MedicalAppointments.BusinessObjects.DTOs;
public class CreateMedicalAppointmentDto
{
    public CreateMedicalAppointmentDto(DateTime dateTime, Guid patientId,
        Guid doctorId, string reason)
    {
        DateTime = dateTime;
        PatientId = patientId;
        DoctorId = doctorId;
        Reason = reason;
    }

    public DateTime DateTime { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public string Reason { get; init; }
}
