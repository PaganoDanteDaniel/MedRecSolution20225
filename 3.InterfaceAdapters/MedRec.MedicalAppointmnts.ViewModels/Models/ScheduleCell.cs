namespace MedRec.MedicalAppointments.ViewModels.Models;

public class ScheduleCell
{
    public DateTime DateTime { get; set; }
    public Appointment Appointment { get; set; }
    public string Key => DateTime.ToString("yyyy-MM-dd HH:mm");
    public bool IsPast => DateTime < DateTime.Now;
}