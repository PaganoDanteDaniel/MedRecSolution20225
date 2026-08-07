namespace MedRec.Identity.BusinessObjects.DTOs;
public class CreateUserDto
{
    public CreateUserDto(string email, string fullName, string temporaryPassword, IReadOnlyList<Guid> roleIds, Guid? doctorId)
    {
        Email = email;
        FullName = fullName;
        TemporaryPassword = temporaryPassword;
        RoleIds = roleIds;
        DoctorId = doctorId;
    }

    public string Email { get; }
    public string FullName { get; }
    public string TemporaryPassword { get; }
    public IReadOnlyList<Guid> RoleIds { get; }
    public Guid? DoctorId { get; }
}
