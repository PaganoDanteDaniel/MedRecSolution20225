using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.ViewModels.Models;

namespace MedRec.Professionals.ViewModels.Orchestration;
public static class ProfessionalMapper
{
    public static CreateProfessionalDto ToCreateDto(CreateProfessionalModel model) =>
        new(model.FirstName, model.LastName, model.Email, model.Phone, model.HireDate, model.Type, model.LicenseNumber, model.SpecialtyId);

    public static UpdateProfessionalDto ToUpdateDto(UpdateProfessionalModel model) =>
        new(model.Id, model.FirstName, model.LastName, model.Phone, model.Type, model.LicenseNumber, model.SpecialtyId, model.RowVersion);

    public static ProfessionalModel ToModel(ProfessionalDto dto) => new()
    {
        Id = dto.Id,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Phone = dto.Phone,
        HireDate = dto.HireDate,
        Type = dto.Type,
        LicenseNumber = dto.LicenseNumber,
        SpecialtyId = dto.SpecialtyId
    };
}
