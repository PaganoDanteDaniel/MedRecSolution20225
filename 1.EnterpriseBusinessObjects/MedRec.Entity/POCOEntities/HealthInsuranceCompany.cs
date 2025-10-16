namespace MedRec.Entity.POCOEntities;
public class HealthInsuranceCompany
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Acronym { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
