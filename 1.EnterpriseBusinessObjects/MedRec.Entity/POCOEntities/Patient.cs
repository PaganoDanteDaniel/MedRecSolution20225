using MedRec.Entity.Enums;

namespace MedRec.Entity.POCOEntities;
public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DocumentNumber { get; set; }
    public string Address { get; set; }
    public Guid? CityId { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public BiologicalSex BiologicalSexId { get; set; } = BiologicalSex.Unknown;
    public Guid? HealthInsuranceId { get; set; }
    public string HealthInsuranceMemberNumber { get; set; }
    public string HealthInsuranceCard { get; set; }
    public string HealthInsurancePlan { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }

    public string FullName => $"{LastName}, {FirstName}";
}
