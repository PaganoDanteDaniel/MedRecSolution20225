using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface IUpdatePatientInputPort
{
    Task Handle(UpdatePatientDto editPatient, CancellationToken cancellationToken = default);
}
