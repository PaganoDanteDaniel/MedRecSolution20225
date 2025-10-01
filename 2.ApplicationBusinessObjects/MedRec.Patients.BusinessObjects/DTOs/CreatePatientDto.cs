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
        Guid? insuranceHealthCompanyId = null,
        string insuranceHealthMemberNumber = "",
        string insuranceHealthCard = "",
        string insuranceHealthPlan = "")
    {
        FirstName = firstName;
        LastName = lastName;
        DocumentNumber = documentNumber;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        Email = email;
        InsuranceHealthCompanyId = insuranceHealthCompanyId;
        InsuranceHealthMemberNumber = insuranceHealthMemberNumber ?? string.Empty;
        InsuranceHealthCard = insuranceHealthCard ?? string.Empty;
        InsuranceHealthPlan = insuranceHealthPlan ?? string.Empty;
    }

    // Campos obligatorios
    public string FirstName { get; }                 // Nombre
    public string LastName { get; }                  // Apellido
    public string DocumentNumber { get; }            // Documento
    public string PhoneNumber { get; }               // Teléfono
    public DateTime DateOfBirth { get; }             // Fecha de nacimiento

    // Campos opcionales relacionados con seguro médico
    public string Email { get; }
    public Guid? InsuranceHealthCompanyId { get; }   // Compañía de seguro
    public string InsuranceHealthMemberNumber { get; } // Número de afiliado
    public string InsuranceHealthCard { get; }       // Carnet / tarjeta
    public string InsuranceHealthPlan { get; }       // Plan de cobertura

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
            HealthInsuranceId = dto.InsuranceHealthCompanyId,
            HealthInsuranceCard = dto.InsuranceHealthCard,
            HealthInsuranceMemberNumber = dto.InsuranceHealthMemberNumber,
            HealthInsurancePlan = dto.InsuranceHealthPlan
        };
    }
}

