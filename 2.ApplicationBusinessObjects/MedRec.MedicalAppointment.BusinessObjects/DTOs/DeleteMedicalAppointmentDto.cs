namespace MedRec.MedicalAppointments.BusinessObjects.DTOs;
public class DeleteMedicalAppointmentDto
{
    public DeleteMedicalAppointmentDto(Guid id, bool isDeleted)
    {
        Id = id;
        IsDeleted = isDeleted;
    }

    public Guid Id { get; init; }
    public bool IsDeleted { get; init; }
}
