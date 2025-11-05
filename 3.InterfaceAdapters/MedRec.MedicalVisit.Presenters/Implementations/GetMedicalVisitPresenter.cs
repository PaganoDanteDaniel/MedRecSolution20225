using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalVisit.Presenters.Implementations;
internal class GetMedicalVisitPresenter : IGetMedicalVisitOutputPort
{
    public GetMedicalVisitDto MedicalVisit { get; private set; }

    public IEnumerable<ValidationError> ValidationErrors { get; private set; }

    public ErrorInfo ErrorMessage { get; private set; }

    public Task ErrorAsync(ErrorInfo message)
    {
        ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task Handle(PatientMedicalVisit medicalVisit, CancellationToken cts = default)
    {
        MedicalVisit = new GetMedicalVisitDto
        {
            Id = medicalVisit.Id,
            MedicalHistoryId = medicalVisit.MedicalHistoryId,
            VisitDate = medicalVisit.VisitDate,
            Reason = medicalVisit.Reason,
            Diagnosis = medicalVisit.Diagnosis,
            Treatment = medicalVisit.Treatment,
            SystolicPressure = medicalVisit.SystolicPressure,
            DiastolicPressure = medicalVisit.DiastolicPressure,
            PulsePerMinute = medicalVisit.PulsePerMinute,
            Temperature = medicalVisit.Temperature,
            Notes = medicalVisit.Notes,
            RowVersion = medicalVisit.RowVersion
        };
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        ValidationErrors = errors;
        return Task.CompletedTask;
    }
}
