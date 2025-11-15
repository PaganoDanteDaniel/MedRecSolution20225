using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalVisit.Presenters.Implementations;
internal class GetMedicalVisitPresenter : IGetMedicalVisitOutputPort
{
    private GetMedicalVisitDto _medicalVisit;
    private ErrorInfo? _errorMessage;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();

    public GetMedicalVisitDto MedicalVisit => _medicalVisit;
    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;
    public ErrorInfo ErrorMessage => _errorMessage;

    public Task ErrorAsync(ErrorInfo message)
    {
        _errorMessage = message ?? new ErrorInfo("Error desconocido.");
        _validationErrors = Array.Empty<ValidationError>();
        _medicalVisit = null;
        return Task.CompletedTask;
    }
    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        _validationErrors = (errors ?? Enumerable.Empty<ValidationError>()).ToArray();
        _errorMessage = null;
        _medicalVisit = null;
        return Task.CompletedTask;
    }
    public Task Handle(PatientMedicalVisit medicalVisit, CancellationToken cts = default)
    {
        _errorMessage = null;
        _validationErrors = Array.Empty<ValidationError>();
        _medicalVisit = new GetMedicalVisitDto
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
}
