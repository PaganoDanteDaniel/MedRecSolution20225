using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.MedicalVisit.Presenters.Implementations;
internal class MedicalVisitSummaryListPresenter : IMedicalVisitSummaryListOutputPort
{
    public IEnumerable<MedicalVisitSummaryDto> ListMedicalVisitSummary { get; private set; }

    public IEnumerable<ValidationError> ValidationErrors { get; private set; }

    public ErrorInfo ErrorMessage { get; private set; }

    public Task ErrorAsync(ErrorInfo message)
    {
        ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task Handle(IEnumerable<PatientMedicalVisit> listMedicalVisit)
    {
        var dto = listMedicalVisit.Select(v => new MedicalVisitSummaryDto
        {
            Id = v.Id,
            VisitDate = v.VisitDate,
            Reason = v.Reason,
            Diagnosis = v.Diagnosis,
            Treatment = v.Treatment,
            Notes = v.Notes
        }).ToList();

        ListMedicalVisitSummary = dto;

        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        ValidationErrors = errors.ToList();
        return Task.CompletedTask;
    }
}
