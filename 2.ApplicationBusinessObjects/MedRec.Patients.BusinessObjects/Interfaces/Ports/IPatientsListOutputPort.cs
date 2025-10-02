using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IPatientsListOutputPort : ICommonOutputPort
{
    int TotalRecords { get; }
    IEnumerable<PatientSummaryDto> Patients { get; }
    Task Handle(IEnumerable<Patient> patientList, int totalRecord, CancellationToken cancellationToken = default);
}
