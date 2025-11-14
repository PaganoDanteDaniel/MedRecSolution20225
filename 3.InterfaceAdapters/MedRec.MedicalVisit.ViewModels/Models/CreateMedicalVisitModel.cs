namespace MedRec.MedicalVisit.ViewModels.Models;
public class CreateMedicalVisitModel
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string HealthInsuranceName { get; set; }
    public string Acronym { get; set; }
    public string HealthInsuranceMemberNumber { get; set; }
    public string HealthInsuranceCard { get; set; }
    public string HealthInsurancePlan { get; set; }

    public Guid Id { get; set; }
    public Guid MedicalHistoryId { get; set; }
    public DateTime VisitDate { get; set; } = DateTime.Now;
    public string Reason { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public int? SystolicPressure { get; set; }
    public int? DiastolicPressure { get; set; }
    public int? PulsePerMinute { get; set; }
    public double? Temperature { get; set; }
    public string Notes { get; set; }
    public byte[] RowVersion { get; set; }

    //public static explicit operator CreateMedicalVisitDto(CreateMedicalVisitModel model)
    //{
    //    return new CreateMedicalVisitDto
    //    {
    //        //Id = model.Id,
    //        MedicalHistoryId = model.MedicalHistoryId,
    //        VisitDate = model.VisitDate,
    //        Reason = (model.Reason ?? string.Empty).ToUpperInvariant(),
    //        Diagnosis = (model.Diagnosis ?? string.Empty).ToUpperInvariant(),
    //        Treatment = (model.Treatment ?? string.Empty).ToUpperInvariant(),
    //        SystolicPressure = model.SystolicPressure,
    //        DiastolicPressure = model.DiastolicPressure,
    //        PulsePerMinute = model.PulsePerMinute,
    //        Temperature = model.Temperature,
    //        Notes = (model.Notes ?? string.Empty).ToUpperInvariant(),
    //        RowVersion = model.RowVersion
    //    };
    //}
    public CreateMedicalVisitModel Clone()
    {
        return new CreateMedicalVisitModel
        {
            MedicalHistoryId = this.MedicalHistoryId,
            Reason = this.Reason?.ToUpperInvariant(),
            Diagnosis = this.Diagnosis?.ToUpperInvariant(),
            Treatment = this.Treatment?.ToUpperInvariant(),
            SystolicPressure = this.SystolicPressure,
            DiastolicPressure = this.DiastolicPressure,
            PulsePerMinute = this.PulsePerMinute,
            Temperature = this.Temperature,
            Notes = this.Notes?.ToUpperInvariant(),
            RowVersion = this.RowVersion
        };
    }
}
