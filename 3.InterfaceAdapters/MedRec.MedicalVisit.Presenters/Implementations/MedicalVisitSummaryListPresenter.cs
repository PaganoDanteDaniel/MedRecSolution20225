using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalVisit.Presenters.Implementations;
internal class MedicalVisitSummaryListPresenter : IMedicalVisitSummaryListOutputPort
{
    private IReadOnlyList<MedicalVisitSummaryDto>? _listMedicalVisitSummary = Array.Empty<MedicalVisitSummaryDto>();
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    private ErrorInfo? _errorMessage;

    public IEnumerable<MedicalVisitSummaryDto> ListMedicalVisitSummary => _listMedicalVisitSummary;
    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;
    public ErrorInfo ErrorMessage => _errorMessage;



    public Task ErrorAsync(ErrorInfo message)
    {
        _errorMessage = message ?? new ErrorInfo("Error desconocido.");
        _validationErrors = Array.Empty<ValidationError>();
        _listMedicalVisitSummary = Array.Empty<MedicalVisitSummaryDto>();
        return Task.CompletedTask;
    }
    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        _validationErrors = (errors ?? Enumerable.Empty<ValidationError>()).ToArray();
        _errorMessage = null;
        _listMedicalVisitSummary = Array.Empty<MedicalVisitSummaryDto>();
        return Task.CompletedTask;
    }
    public Task Handle(IEnumerable<PatientMedicalVisit> listMedicalVisit)
    {
        _validationErrors = Array.Empty<ValidationError>();
        _errorMessage = null;
        var dto = listMedicalVisit.Select(v => new MedicalVisitSummaryDto
        {
            Id = v.Id,
            VisitDate = v.VisitDate,
            Reason = v.Reason,
            Diagnosis = v.Diagnosis,
            Treatment = v.Treatment,
            Notes = v.Notes
        }).ToList();

        _listMedicalVisitSummary = dto;

        return Task.CompletedTask;
    }
}
