using System.ComponentModel;

namespace MedRec.Entity.Enums;
public enum BiologicalSex
{
    [Description("NO ESPECIFICADO")]
    NotSpecified = 1,
    [Description("MASCULINO")]
    Male = 2,
    [Description("FEMENINO")]
    Female = 3,
    [Description("INTERSEXUAL")]
    Intersex = 4,
    [Description("DESCONOCIDO")]
    Unknown = 5
}
