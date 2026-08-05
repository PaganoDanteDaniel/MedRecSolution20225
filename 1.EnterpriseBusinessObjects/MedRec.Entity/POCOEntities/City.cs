using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class City : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProvinceId { get; set; }
    public string CityName { get; set; }
    public string CityCode { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
