namespace MedRec.MedicalAppointments.ViewModels.Models;

public class Appointment
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProfessionalId { get; set; }
    public string PatientLastName { get; set; }
    public string PatientFirstName { get; set; }
    public string Phone { get; set; }
    public string Reason { get; set; }
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; }

    public string PatientName
    {
        get { return $"{PatientLastName}, {PatientFirstName}"; }
        set
        {
            var partes = value.Split(new[] { ',' }, 2);
            PatientLastName = partes[0];
            PatientFirstName = partes[1];
        }
    }
}