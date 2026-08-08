using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.ViewModels.Models;
public class UpdateUserModel
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public List<Guid> RoleIds { get; set; } = new();
    public Guid? DoctorId { get; set; }

    public static explicit operator UpdateUserDto(UpdateUserModel model) =>
        new(model.UserId, model.FullName, model.RoleIds, model.DoctorId);
}
