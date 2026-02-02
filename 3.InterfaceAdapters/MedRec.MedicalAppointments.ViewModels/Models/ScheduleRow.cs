namespace MedRec.MedicalAppointments.ViewModels.Models;
public class ScheduleRow
{
    public string Time { get; set; } = "";
    public List<ScheduleCell> Cells { get; set; } = new();
}
