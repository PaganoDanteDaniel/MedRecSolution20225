using MedRec.Entity.Enums;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Validators;

namespace MedRec.Professionals.UseCases.Tests;

public class UpdateProfessionalValidatorTests
{
    private static UpdateProfessionalDto Build(ProfessionalType type, string? licenseNumber = null, Guid? specialtyId = null) =>
        new(Guid.NewGuid(), "Ana", "García", "1140001111", type, licenseNumber, specialtyId, new byte[] { 1, 2, 3 });

    [Fact]
    public void Validate_ShouldFail_WhenDoctorHasNoSpecialty()
    {
        var dto = Build(ProfessionalType.Doctor, licenseNumber: "MP123", specialtyId: null);
        var errors = UpdateProfessionalValidator.Validate(dto);
        Assert.Contains(errors, e => e.PropertyName == nameof(UpdateProfessionalDto.SpecialtyId));
    }

    [Fact]
    public void Validate_ShouldPass_WhenDoctorHasLicenseAndSpecialty()
    {
        var dto = Build(ProfessionalType.Doctor, licenseNumber: "MP123", specialtyId: Guid.NewGuid());
        var errors = UpdateProfessionalValidator.Validate(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenNurseHasNoLicense()
    {
        var dto = Build(ProfessionalType.Nurse, licenseNumber: null);
        var errors = UpdateProfessionalValidator.Validate(dto);
        Assert.Contains(errors, e => e.PropertyName == nameof(UpdateProfessionalDto.LicenseNumber));
    }

    [Fact]
    public void Validate_ShouldPass_WhenNurseHasLicense()
    {
        var dto = Build(ProfessionalType.Nurse, licenseNumber: "EN456");
        var errors = UpdateProfessionalValidator.Validate(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenReceptionistHasNoLicenseNorSpecialty()
    {
        var dto = Build(ProfessionalType.Receptionist);
        var errors = UpdateProfessionalValidator.Validate(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenAdministratorHasNoLicenseNorSpecialty()
    {
        var dto = Build(ProfessionalType.Administrator);
        var errors = UpdateProfessionalValidator.Validate(dto);
        Assert.Empty(errors);
    }
}
