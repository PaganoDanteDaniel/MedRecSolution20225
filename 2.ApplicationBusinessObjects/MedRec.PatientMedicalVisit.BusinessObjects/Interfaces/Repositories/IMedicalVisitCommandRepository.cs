using MedRec.PatientMedicalVisit.BusinessObjects.DTOs;

namespace MedRec.MedicalVisit.BusinessObjects.Repository;
public interface IMedicalVisitCommandRepository
{
    Task AddMedicalVisit(CreateMedicalVisitDto medicalVisit, CancellationToken cts = default);
}
