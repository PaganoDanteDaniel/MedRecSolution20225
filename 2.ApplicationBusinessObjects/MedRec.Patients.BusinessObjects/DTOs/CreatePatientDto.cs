using MedRec.Entity.POCOEntities;

namespace MedRec.Patients.BusinessObjects.DTOs;
public class CreatePatientDto
{
    public CreatePatientDto(string firstName,
        string lastName,
        string documentNumber,
        string phoneNumber,
        DateTime dateOfBirth,
        string email,
        Guid? healthInsuranceId = null,
        string healthInsuranceMemberNumber = "",
        string healthInsuranceCard = "",
        string healthInsurancePlan = "")
    {
        FirstName = firstName;
        LastName = lastName;
        DocumentNumber = documentNumber;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        Email = email;
        HealthInsuranceId = healthInsuranceId;
        HealthInsuranceMemberNumber = healthInsuranceMemberNumber ?? string.Empty;
        HealthInsuranceCard = healthInsuranceCard ?? string.Empty;
        HealthInsurancePlan = healthInsurancePlan ?? string.Empty;
    }

    // Campos obligatorios
    public string FirstName { get; }                 // Nombre
    public string LastName { get; }                  // Apellido
    public string DocumentNumber { get; }            // Documento
    public string PhoneNumber { get; }               // Teléfono
    public DateTime DateOfBirth { get; }             // Fecha de nacimiento

    // Campos opcionales relacionados con seguro médico
    public string Email { get; }
    public Guid? HealthInsuranceId { get; }   // Compañía de seguro
    public string HealthInsuranceMemberNumber { get; } // Número de afiliado
    public string HealthInsuranceCard { get; }       // Carnet / tarjeta
    public string HealthInsurancePlan { get; }       // Plan de cobertura

    public static explicit operator Patient(CreatePatientDto dto)
    {
        return new Patient()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DocumentNumber = dto.DocumentNumber,
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth,
            Email = dto.Email,
            HealthInsuranceId = dto.HealthInsuranceId,
            HealthInsuranceCard = dto.HealthInsuranceCard,
            HealthInsuranceMemberNumber = dto.HealthInsuranceMemberNumber,
            HealthInsurancePlan = dto.HealthInsurancePlan
        };
    }
}

