namespace MedRec.MedicalVisit.ViewModels.Models;

public class CreateMedicalVisitModel : PatientModel
{
    public Guid Id { get; set; }
    public Guid SpecialtyId { get; set; }
    public Guid MedicalHistoryId { get; set; }
    public DateTime VisitDate { get; set; } = DateTime.Now;
    public string Reason { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public string Notes { get; set; }
    public byte[] RowVersion { get; set; }
    public List<DynamicFieldValueModel> DynamicFields { get; set; }

    public CreateMedicalVisitModel Clone()
    {
        return new CreateMedicalVisitModel
        {
            // NUEVO: Copiar propiedades heredadas de PatientModel
            PatientId = this.PatientId,
            SpecialtyId = this.SpecialtyId,
            FullName = this.FullName,
            DateOfBirth = this.DateOfBirth,
            HealthInsuranceName = this.HealthInsuranceName,
            Acronym = this.Acronym,
            HealthInsuranceMemberNumber = this.HealthInsuranceMemberNumber,
            HealthInsuranceCard = this.HealthInsuranceCard,
            HealthInsurancePlan = this.HealthInsurancePlan,

            // Propiedades de CreateMedicalVisitModel
            Id = this.Id,
            MedicalHistoryId = this.MedicalHistoryId,
            VisitDate = this.VisitDate,
            Reason = this.Reason?.ToUpperInvariant(),
            Diagnosis = this.Diagnosis?.ToUpperInvariant(),
            Treatment = this.Treatment?.ToUpperInvariant(),
            Notes = this.Notes?.ToUpperInvariant(),
            RowVersion = (byte[])this.RowVersion?.Clone(),
            DynamicFields = this.DynamicFields?.Select(df => df.Clone()).ToList()

        };
    }
}
