using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.Patients.Presenters.Implementations;
internal class PatientsListPresenter : IPatientsListOutputPort
{
    public int TotalRecords { get; private set; }

    public IEnumerable<PatientSummaryDto> Patients { get; private set; }

    public IEnumerable<ValidationError> ValidationErrors { get; private set; }

    public ErrorInfo ErrorMessage { get; private set; }

    public Task ErrorAsync(ErrorInfo message)
    {
        ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task Handle(IEnumerable<Patient> patientList, int totalRecord, CancellationToken cancellationToken = default)
    {
        Patients = patientList.Select(p => (new PatientSummaryDto(
            p.Id,
            p.FirstName,
            p.LastName,
            p.DocumentNumber,
            p.PhoneNumber,
            p.Email,
            p.DateOfBirth))).ToList();
        TotalRecords = totalRecord;

        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        ValidationErrors = errors.ToList();
        return Task.CompletedTask;
    }
}
