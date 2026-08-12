namespace MedRec.MedicalAppointments.BusinessObjects.DTOs;
public class MedicalAppointmentDto
{
    public MedicalAppointmentDto(Guid id, DateTime dateTime,
        Guid patientId, Guid doctorId,
        string reason, byte[] rowVersion,
        bool isDeleted, string patientFirstName = "",
        string patientLastName = "", string patientPhoneNumber = "")
    {
        Id = id;
        DateTime = dateTime;
        PatientId = patientId;
        ProfessionalId = doctorId;
        Reason = reason;
        RowVersion = rowVersion;
        IsDeleted = isDeleted;
        PatientFirstName = patientFirstName;
        PatientLastName = patientLastName;
        PatientPhoneNumber = patientPhoneNumber;
    }

    public Guid Id { get; init; }
    public DateTime DateTime { get; init; }
    public Guid PatientId { get; init; }
    public Guid ProfessionalId { get; init; }
    public string Reason { get; init; }
    public byte[] RowVersion { get; init; }
    public bool IsDeleted { get; init; }

    public string PatientFirstName { get; init; } = string.Empty;
    public string PatientLastName { get; init; } = string.Empty;
    public string PatientPhoneNumber { get; init; } = string.Empty;

}
