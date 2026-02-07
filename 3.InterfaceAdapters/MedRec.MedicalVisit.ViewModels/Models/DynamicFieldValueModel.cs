namespace MedRec.MedicalVisit.ViewModels.Models;

public class DynamicFieldValueModel
{
    public Guid FieldDefinitionId { get; set; }
    public string? FieldValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BooleanValue { get; set; }

    /// <summary>
    /// Crea una copia profunda del objeto
    /// </summary>
    public static DynamicFieldValueModel Clone(DynamicFieldValueModel source)
    {
        if (source == null)
            return null;

        return new DynamicFieldValueModel
        {
            FieldDefinitionId = source.FieldDefinitionId,
            FieldValue = source.FieldValue,
            NumericValue = source.NumericValue,
            DateValue = source.DateValue,
            BooleanValue = source.BooleanValue
        };
    }

    /// <summary>
    /// Método de instancia para conveniencia
    /// </summary>
    public DynamicFieldValueModel Clone()
    {
        return Clone(this);
    }
}