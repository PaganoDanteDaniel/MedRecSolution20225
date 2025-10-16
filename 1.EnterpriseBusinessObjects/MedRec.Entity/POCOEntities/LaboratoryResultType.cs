namespace MedRec.Entity.POCOEntities;
public class LaboratoryResultType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ResultName { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
