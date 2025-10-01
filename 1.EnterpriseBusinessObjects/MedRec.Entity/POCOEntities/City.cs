namespace MedRec.Entity.POCOEntities;
public class City
{
    public Guid Id { get; set; }
    public Guid ProvinceId { get; set; }
    public string CityName { get; set; }
    public string CityCode { get; set; }
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; }

}
