namespace MedRec.PatientMedicalVisit.BusinessObjects.Validator;
using global::MedRec.PatientMedicalVisit.BusinessObjects.Constraints;
using global::MedRec.PatientMedicalVisit.BusinessObjects.DTOs;
using global::MedRec.Validator.ValueObjects;
using MedRec.PatientMedicalVisit.BusinessObjects.Resources;
using MedRec.Shared.Gruards;

public static class CreateMedicalVisitValidator
{
    public static IReadOnlyList<ValidationError> Validate(CreateMedicalVisitDto visit, int? patientAge = null)
    {
        if (visit == null)
            throw new ArgumentNullException(nameof(visit));

        var errors = new List<ValidationError>();

        // MedicalHistoryId
        if (visit.MedicalHistoryId == Guid.Empty)
        {
            errors.Add(new ValidationError(nameof(visit.MedicalHistoryId), MedicalVisitValidatorMessages.PatientId_Required));
        }

        // VisitDate
        var dateValidation = Guard.Against(visit.VisitDate, nameof(visit.VisitDate))
            .NotInFuture(MedicalVisitValidatorMessages.VisitDate_NotInFuture);
        errors.AddRange(dateValidation.Errors);

        // Reason
        var reasonValidation = Guard.Against(visit.Reason, nameof(visit.Reason))
            .NotNullOrEmpty(MedicalVisitValidatorMessages.Reason_Required)
            .MaxLength(MedicalVisitConstraints.MaxLengthReason,
                       string.Format(MedicalVisitValidatorMessages.Reason_MaxLength, MedicalVisitConstraints.MaxLengthReason));
        errors.AddRange(reasonValidation.Errors);

        // Diagnosis (opcional, pero con longitud)
        if (!string.IsNullOrWhiteSpace(visit.Diagnosis))
        {
            var diagnosisValidation = Guard.Against(visit.Diagnosis, nameof(visit.Diagnosis))
                .MaxLength(MedicalVisitConstraints.MaxLengthDiagnosis,
                           string.Format(MedicalVisitValidatorMessages.Diagnosis_MaxLength, MedicalVisitConstraints.MaxLengthDiagnosis));
            errors.AddRange(diagnosisValidation.Errors);
        }

        // Treatment (opcional, pero con longitud)
        if (!string.IsNullOrWhiteSpace(visit.Treatment))
        {
            var treatmentValidation = Guard.Against(visit.Treatment, nameof(visit.Treatment))
                .MaxLength(MedicalVisitConstraints.MaxLengthTreatment,
                           string.Format(MedicalVisitValidatorMessages.Treatment_MaxLength, MedicalVisitConstraints.MaxLengthTreatment));
            errors.AddRange(treatmentValidation.Errors);
        }

        // Notes (opcional, pero con longitud)
        if (!string.IsNullOrWhiteSpace(visit.Notes))
        {
            var notesValidation = Guard.Against(visit.Notes, nameof(visit.Notes))
                .MaxLength(MedicalVisitConstraints.MaxLengthNotes,
                           string.Format(MedicalVisitValidatorMessages.Notes_MaxLength, MedicalVisitConstraints.MaxLengthNotes));
            errors.AddRange(notesValidation.Errors);
        }

        // === Validación de signos vitales con límites médicos y edad ===

        // Systolic Pressure
        if (visit.SystolicPressure.HasValue)
        {
            var sys = visit.SystolicPressure.Value;
            if (sys < MedicalVisitConstraints.MinSystolicPressure || sys > MedicalVisitConstraints.MaxSystolicPressure)
            {
                errors.Add(new ValidationError(nameof(visit.SystolicPressure),
                    string.Format(MedicalVisitValidatorMessages.SystolicPressure_OutOfRange,
                        MedicalVisitConstraints.MinSystolicPressure,
                        MedicalVisitConstraints.MaxSystolicPressure)));
            }
        }

        // Diastolic Pressure
        if (visit.DiastolicPressure.HasValue)
        {
            var dia = visit.DiastolicPressure.Value;
            if (dia < MedicalVisitConstraints.MinDiastolicPressure || dia > MedicalVisitConstraints.MaxDiastolicPressure)
            {
                errors.Add(new ValidationError(nameof(visit.DiastolicPressure),
                    string.Format(MedicalVisitValidatorMessages.DiastolicPressure_OutOfRange,
                        MedicalVisitConstraints.MinDiastolicPressure,
                        MedicalVisitConstraints.MaxDiastolicPressure)));
            }
        }

        // === Pulso: validación adaptativa ===
        if (visit.PulsePerMinute.HasValue)
        {
            var pulse = visit.PulsePerMinute.Value;
            int minPulse = MedicalVisitConstraints.MinPulsePerMinute;
            int maxPulse;

            if (patientAge.HasValue)
            {
                var maxHr = CalculateMaxHeartRate(patientAge.Value);
                // Permitir hasta 110% de FCmáx, pero no más del límite absoluto
                maxPulse = Math.Min((int)(maxHr * 1.1), MedicalVisitConstraints.MaxPulsePerMinute);

                if (pulse < minPulse || pulse > maxPulse)
                {
                    errors.Add(new ValidationError(nameof(visit.PulsePerMinute),
                        string.Format(MedicalVisitValidatorMessages.Pulse_OutOfRange_WithAge,
                            minPulse, maxPulse, patientAge.Value, maxHr)));
                }
            }
            else
            {
                // Sin edad: usar rango general seguro
                maxPulse = MedicalVisitConstraints.MaxPulsePerMinute;
                if (pulse < minPulse || pulse > maxPulse)
                {
                    errors.Add(new ValidationError(nameof(visit.PulsePerMinute),
                        string.Format(MedicalVisitValidatorMessages.Pulse_OutOfRange_General,
                            minPulse, maxPulse)));
                }
            }
        }


        // Temperature
        if (visit.Temperature.HasValue)
        {
            var temp = visit.Temperature.Value;
            if (temp < MedicalVisitConstraints.MinTemperature || temp > MedicalVisitConstraints.MaxTemperature)
            {
                errors.Add(new ValidationError(nameof(visit.Temperature),
                    string.Format(MedicalVisitValidatorMessages.Temperature_OutOfRange,
                        MedicalVisitConstraints.MinTemperature,
                        MedicalVisitConstraints.MaxTemperature)));
            }
        }

        return errors;
    }

    // Fórmula clásica: FCmáx = 220 - edad
    private static int CalculateMaxHeartRate(int age)
    {
        return Math.Max(60, 220 - age); // nunca menor a 60
    }
}
