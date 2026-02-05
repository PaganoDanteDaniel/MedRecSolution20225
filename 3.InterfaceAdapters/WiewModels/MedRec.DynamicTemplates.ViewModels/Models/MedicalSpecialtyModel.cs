namespace MedRec.DynamicTemplates.ViewModels.Models;
public class MedicalSpecialtyModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
}
