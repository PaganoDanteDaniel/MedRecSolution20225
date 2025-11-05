using MedRec.BusinessObjects.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IPatientForMedicalVisitOutputPort : ICommonOutputPort
{
    PatientForMedicalVisitDto DataPatient { get; }
    Task Handle(Patient dataPatient, HealthInsuranceCompany healthInsurance = null, CancellationToken ct = default);
}
