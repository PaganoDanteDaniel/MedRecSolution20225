using MedRec.Patients.BusinessObjects.DTOs;

namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
public interface ICreatePatientInputPort
{
    Task HandleAsync(CreatePatientDto dto, CancellationToken ct = default);
}
