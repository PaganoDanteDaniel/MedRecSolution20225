namespace MedRec.DynamicTemplates.BusinessObjects.Constraints;

public static class DynamicTemplatesConstraints
{
    // MedicalSpecialty
    public const int SpecialtyNameMaxLength = 100;
    public const int SpecialtyDescriptionMaxLength = 500;
    public const int SpecialtyIconMaxLength = 50;

    // TemplateFieldDefinition
    public const int FieldNameMaxLength = 100;
    public const int FieldLabelMaxLength = 200;
    public const int FieldTypeMaxLength = 50;
    public const int CategoryMaxLength = 100;
    public const int DefaultValueMaxLength = 500;
    public const int UnitMaxLength = 50;
    public const int HelpTextMaxLength = 500;

    // DynamicFieldValue
    public const int FieldValueMaxLength = 5000;
}