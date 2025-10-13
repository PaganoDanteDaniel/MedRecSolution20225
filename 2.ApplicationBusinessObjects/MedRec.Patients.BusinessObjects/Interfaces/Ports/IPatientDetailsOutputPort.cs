using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IPatientDetailsOutputPort : ICommonOutputPort
{
    PatientDetailDto PatientDetails { get; }
    Task Handle(Patient patientEntity, HealthInsuranceCompany healthInsurance = null, CancellationToken cancellationToken = default);
}
