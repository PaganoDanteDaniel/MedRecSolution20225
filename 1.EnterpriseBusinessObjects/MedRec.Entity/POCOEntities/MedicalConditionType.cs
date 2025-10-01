namespace MedRec.Entity.POCOEntities;
public class MedicalConditionType
{
    public Guid Id { get; set; }                         // Antes MedicalAntecedentTypeId
    public string Name { get; set; }                   // Antes MedicalAntecedentTypeName
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
