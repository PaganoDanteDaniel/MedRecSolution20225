using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IPatientDetailsOutputPort : IBaseOutputPort
{
    OperationResult<PatientDetailDto> Result { get; }
    Task Handle(Patient patientEntity, HealthInsuranceCompany healthInsurance = null, CancellationToken cancellationToken = default);
}
