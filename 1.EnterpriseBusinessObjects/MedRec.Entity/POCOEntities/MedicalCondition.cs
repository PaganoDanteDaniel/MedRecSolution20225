namespace MedRec.Entity.POCOEntities;
public class MedicalCondition
{
    public Guid Id { get; set; }                         // Antes MedicalAntecedentId
    public Guid ConditionTypeId { get; set; }           // Antes MedicalAntecedentTypeId
    public string Name { get; set; }                   // Antes MedicalAntecedentName
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
}
