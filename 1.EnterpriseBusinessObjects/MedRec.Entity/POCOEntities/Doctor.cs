namespace MedRec.Entity.POCOEntities;
public class Doctor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string LicenseNumber { get; set; } = string.Empty; // e.g., "MP 123456"
    public string Specialty { get; set; } = string.Empty;      // Consider a separate Specialty entity if needed
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = true;
    public byte[] RowVersion { get; set; }

    // Computed property for display purposes
    public string FullName => $"{LastName}, {FirstName}";
}
