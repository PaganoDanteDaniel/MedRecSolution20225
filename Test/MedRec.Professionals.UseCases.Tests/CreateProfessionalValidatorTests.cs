using MedRec.Entity.Enums;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Validators;

namespace MedRec.Professionals.UseCases.Tests;

public class CreateProfessionalValidatorTests
{
    private static CreateProfessionalDto Build(ProfessionalType type, string? licenseNumber = null, Guid? specialtyId = null) =>
        new("Ana", "García", "ana@medrec.local", "1140001111", DateTime.Today, type, licenseNumber, specialtyId);

    [Fact]
    public void Validate_ShouldFail_WhenDoctorHasNoSpecialty()
    {
        var dto = Build(ProfessionalType.Doctor, licenseNumber: "MP123", specialtyId: null);
        var errors = CreateProfessionalValidator.Validate(dto);
        Assert.Contains(errors, e => e.PropertyName == nameof(CreateProfessionalDto.SpecialtyId));
    }

    [Fact]
    public void Validate_ShouldPass_WhenDoctorHasLicenseAndSpecialty()
    {
        var dto = Build(ProfessionalType.Doctor, licenseNumber: "MP123", specialtyId: Guid.NewGuid());
        var errors = CreateProfessionalValidator.Validate(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenNurseHasNoLicense()
    {
        var dto = Build(ProfessionalType.Nurse, licenseNumber: null);
        var errors = CreateProfessionalValidator.Validate(dto);
        Assert.Contains(errors, e => e.PropertyName == nameof(CreateProfessionalDto.LicenseNumber));
    }

    [Fact]
    public void Validate_ShouldPass_WhenNurseHasLicense()
    {
        var dto = Build(ProfessionalType.Nurse, licenseNumber: "EN456");
        var errors = CreateProfessionalValidator.Validate(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenReceptionistHasNoLicenseNorSpecialty()
    {
        var dto = Build(ProfessionalType.Receptionist);
        var errors = CreateProfessionalValidator.Validate(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenAdministratorHasNoLicenseNorSpecialty()
    {
        var dto = Build(ProfessionalType.Administrator);
        var errors = CreateProfessionalValidator.Validate(dto);
        Assert.Empty(errors);
    }
}
