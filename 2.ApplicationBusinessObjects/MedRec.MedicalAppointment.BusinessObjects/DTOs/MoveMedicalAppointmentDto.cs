namespace MedRec.MedicalAppointments.BusinessObjects.DTOs;
public class MoveMedicalAppointmentDto
{
    public MoveMedicalAppointmentDto(Guid id, DateTime dateTime, byte[] rowVersion)
    {
        Id = id;
        DateTime = dateTime;
        RowVersion = rowVersion;
    }

    public Guid Id { get; init; }
    public DateTime DateTime { get; init; }
    public byte[] RowVersion { get; init; }
}
