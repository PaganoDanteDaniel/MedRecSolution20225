using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IPatientsListOutputPort : IBaseOutputPort
{
    OperationResult<int> TotalRecords { get; }
    OperationResult<IEnumerable<PatientSummaryDto>> Result { get; }
    Task Handle(IEnumerable<Patient> patientList, int totalRecord, CancellationToken cancellationToken = default);
}
