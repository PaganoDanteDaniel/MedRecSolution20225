using System.ComponentModel;

namespace MedRec.Entity.Enums;
public enum BiologicalSex
{
    [Description("No especificado")]
    NotSpecified = 1,

    [Description("Masculino")]
    Male = 2,

    [Description("Femenino")]
    Female = 3,

    [Description("Intersexual")]
    Intersex = 4,

    [Description("Desconocido")]
    Unknown = 5
}
