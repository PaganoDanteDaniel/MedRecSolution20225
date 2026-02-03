using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.Repositories.Interfaces;

namespace MedRec.MedicalVisit.Repositories.Implementations;
internal class MedicalVisitCommandRepositoryUoW(
    IMedicalVisitCommandDataContext commandsDb) :
    IMedicalVisitCommandRepositoryUoW
{
    public async Task Create(PatientMedicalVisit medicalVisit, CancellationToken ct = default) =>
       await commandsDb.CreateAsync(medicalVisit, ct);
    public async Task<Guid> CreateMedicalHistory(Guid patientId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        var medHist = new PatientMedicalHistory { PatientId = patientId };

        await commandsDb.CreateMedicalHistoryAsync(medHist, cts);

        return medHist.Id;
    }
    public async Task Update(PatientMedicalVisit medicalVisit, CancellationToken cts = default) =>
    await commandsDb.UpdateAsync(medicalVisit, cts);
}
