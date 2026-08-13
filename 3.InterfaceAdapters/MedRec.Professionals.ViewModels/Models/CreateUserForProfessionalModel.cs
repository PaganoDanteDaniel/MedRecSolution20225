namespace MedRec.Professionals.ViewModels.Models;
public class CreateUserForProfessionalModel
{
    public string TemporaryPassword { get; set; } = string.Empty;
    public List<Guid> RoleIds { get; set; } = new();
}
