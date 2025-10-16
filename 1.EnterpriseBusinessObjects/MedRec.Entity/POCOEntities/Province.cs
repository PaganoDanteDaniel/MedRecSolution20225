namespace MedRec.Entity.POCOEntities;
public class Province
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
