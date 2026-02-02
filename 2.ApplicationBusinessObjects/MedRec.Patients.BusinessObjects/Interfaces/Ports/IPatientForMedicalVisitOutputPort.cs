using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IPatientForMedicalVisitOutputPort : IBaseOutputPort
{
    // Resultado unificado
    OperationResult<PatientForMedicalVisitDto> Result { get; }

    // El interactor enviará la entidad dominio (y opcional la obra social) para que el presenter adapte
    Task Handle(Patient patient, HealthInsuranceCompany? insurance, CancellationToken ct = default);
}
