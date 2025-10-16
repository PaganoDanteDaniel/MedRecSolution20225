using System.ComponentModel;

namespace MedRec.Entity.Enums;
public enum VisitReason
{
    [Description("OTRO")]
    Other = 1,

    [Description("CHEQUEO CARDIOLÓGICO")]
    CardiacCheckup = 2,

    [Description("CONTROL DE PRESIÓN ARTERIAL")]
    BloodPressureCheck = 3,

    [Description("CONTROL POST-INFARTO")]
    PostInfarctionFollowUp = 4,

    [Description("DIFICULTAD PARA RESPIRAR")]
    ShortnessOfBreath = 5,

    [Description("DOLOR EN EL PECHO")]
    ChestPain = 6,

    [Description("ELECTROCARDIOGRAMA")]
    ECGRequest = 7,

    [Description("ERGOMETRÍA")]
    StressTestRequest = 8,

    [Description("EVALUACIÓN POR DESMAYO")]
    SyncopeEvaluation = 9,

    [Description("EVALUACIÓN PREOPERATORIA")]
    PreoperativeAssessment = 10,

    [Description("HOLTER ECG")]
    HolterECG = 11,

    [Description("PALPITACIONES")]
    Palpitations = 12,

    [Description("PRESUROMETRÍA")]
    AmbulatoryBPMonitoring = 13,

    [Description("PREQUIRÚRGICO")]
    PreSurgicalEvaluation = 14,

    [Description("SEGUIMIENTO DE MARCAPASOS")]
    PacemakerFollowUp = 15
}


