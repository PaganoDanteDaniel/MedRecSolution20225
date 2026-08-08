using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.ViewModels.Models;
public class CreateUserModel
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public List<Guid> RoleIds { get; set; } = new();
    public Guid? DoctorId { get; set; }

    public static explicit operator CreateUserDto(CreateUserModel model) =>
        new(model.Email, model.FullName, model.TemporaryPassword, model.RoleIds, model.DoctorId);
}
