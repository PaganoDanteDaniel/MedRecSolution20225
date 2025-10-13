namespace MedRec.PatientMedicalVisit.BusinessObjects.Constraints;
/// <summary>
/// Contiene las restricciones y constantes de validación para la entidad PatientMedicalVisit.
/// </summary>
public static class MedicalVisitConstraints
{
    // Longitudes máximas para propiedades de texto
    public const int MaxLengthReason = 500;
    public const int MaxLengthDiagnosis = 1000;
    public const int MaxLengthTreatment = 1000;
    public const int MaxLengthNotes = 2000;

    // Valores por defecto
    public const bool DefaultIsDeleted = false;


    // === Presión arterial (mmHg) ===
    // Durante esfuerzo, la presión sistólica puede superar los 200 mmHg en personas sanas.
    // Valores extremos pero documentados en pruebas de esfuerzo.
    public const int MinSystolicPressure = 60;   // Hipotensión severa (post-paro, shock)
    public const int MaxSystolicPressure = 300;  // Máximo razonable en esfuerzo extremo

    public const int MinDiastolicPressure = 30;  // Muy baja, pero posible en ciertas arritmias o shock
    public const int MaxDiastolicPressure = 140; // Rara vez sube mucho en esfuerzo; >120 ya es muy alta

    // === Pulso (latidos por minuto - lpm) ===
    // Fórmula estimada de FC máxima: 220 - edad. En adultos jóvenes, puede superar 200 lpm.
    // Se permite hasta 250 lpm para cubrir casos extremos (ej. taquicardia supraventricular).
    public const int MinPulsePerMinute = 20;     // Asistolia parcial o error de monitor, pero posible en registros
    public const int MaxPulsePerMinute = 250;    // Límite superior clínicamente plausible

    // === Temperatura corporal (°C) ===
    // La temperatura no suele medirse como parte central del test de esfuerzo,
    // pero se incluye por completitud. El ejercicio intenso puede elevarla ligeramente.
    public const double MinTemperature = 30.0;   // Hipotermia severa (raro, pero posible en emergencias)
    public const double MaxTemperature = 41.5;   // Hipertermia por esfuerzo (golpe de calor); >42°C es letal
}
