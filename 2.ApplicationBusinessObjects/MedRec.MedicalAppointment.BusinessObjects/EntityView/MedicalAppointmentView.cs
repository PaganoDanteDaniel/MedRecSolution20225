namespace MedRec.MedicalAppointments.BusinessObjects.EntityView;
// MedicalAppointmentView.cs
public class MedicalAppointmentView
{
    public Guid Id { get; set; }
    public DateTime AppointmentDateTime { get; set; }

    public string PatientFirstName { get; set; } = string.Empty;
    public string PatientLastName { get; set; } = string.Empty;
    public string PatientPhoneNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    public string DoctorFirstName { get; set; } = string.Empty;
    public string DoctorLastName { get; set; } = string.Empty;

    public byte[] RowVersion { get; set; }
    public bool IsDeleted { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProfessionalId { get; set; }
}