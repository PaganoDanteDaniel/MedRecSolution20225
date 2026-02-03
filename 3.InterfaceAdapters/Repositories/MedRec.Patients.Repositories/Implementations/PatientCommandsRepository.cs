using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.Repositories.Interfaces;

namespace MedRec.Patients.Repositories.Implementations;
internal class PatientCommandsRepository(
        IPatientCommandsDataContext commandsDb) : IPatientCommandsRepository
{

    public async Task Create(Patient patient, CancellationToken cancellationToken = default) =>
        await commandsDb.CreatePatientAsync(patient, cancellationToken);
    public async Task Update(Patient patient, CancellationToken cts = default) =>
        await commandsDb.UpdatePatientAsync(patient, cts);
    public async Task HardDelete(Guid patientId, CancellationToken cts = default) =>
        await commandsDb.HardDeletePatientAsync(patientId, cts);
    public async Task SoftDelete(Patient patient, CancellationToken cancellationToken = default) =>
        await commandsDb.SoftDeletePatientAsync(patient, cancellationToken);

}
