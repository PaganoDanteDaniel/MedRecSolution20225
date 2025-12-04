using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.Patients.Presenters.Implementations;
internal class PatientsListPresenter :
    BaseOutputPort<IEnumerable<PatientSummaryDto>>,
    IPatientsListOutputPort
{
    public OperationResult<int> TotalRecords { get; private set; } =
        OperationResult.Ok<int>(default!);
    public Task Handle(IEnumerable<Patient> patientList, int totalRecord, CancellationToken cancellationToken = default)
    {
        var patients = patientList.Select(p => (new PatientSummaryDto(
            p.Id,
            p.FirstName,
            p.LastName,
            p.DocumentNumber,
            p.PhoneNumber,
            p.Email,
            p.DateOfBirth))).ToList();

        TotalRecords = OperationResult<int>.Ok(totalRecord);

        Result = OperationResult<IEnumerable<PatientSummaryDto>>.Ok(patients);

        return Task.CompletedTask;
    }
}
