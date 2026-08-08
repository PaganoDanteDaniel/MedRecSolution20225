namespace MedRec.Identity.BusinessObjects.DTOs;
public class UpdateUserDto
{
    public UpdateUserDto(Guid userId, string fullName, IReadOnlyList<Guid> roleIds, Guid? doctorId, byte[] rowVersion)
    {
        UserId = userId;
        FullName = fullName;
        RoleIds = roleIds;
        DoctorId = doctorId;
        RowVersion = rowVersion;
    }

    public Guid UserId { get; }
    public string FullName { get; }
    public IReadOnlyList<Guid> RoleIds { get; }
    public Guid? DoctorId { get; }
    public byte[] RowVersion { get; }
}
