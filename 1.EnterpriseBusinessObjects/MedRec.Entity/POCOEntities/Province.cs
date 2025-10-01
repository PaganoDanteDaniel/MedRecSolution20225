namespace MedRec.Entity.POCOEntities;
public class Province
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; }
}
