using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.MedicalVisit.ViewModels.Orchestration.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration;
internal class UpdateMedicalVisitOrchestrator(
    IGetPatientAction getPatientAction,
    IGetMedicalVisitAction getMedicalVisit,
    IUpdateMedicalVisitAction updateMedicalVisit) : IUpdateMedicalVisitOrchestrator
{
    public async Task<OperationResult<bool>> UpdateMedicalVisit(UpdateMedicalVisitModel model, CancellationToken ct) =>
        await updateMedicalVisit.ExecuteAsync(model, ct);

    public async Task<OperationResult<UpdateMedicalVisitModel>> GetMedicalVisit(Guid id, CancellationToken ct) =>
        await getMedicalVisit.ExecuteAsync(id, ct);

    public async Task<OperationResult<UpdateMedicalVisitModel>> GetPatient(Guid id, CancellationToken ct = default)
    {
        var result = await getPatientAction.ExecuteAsync(id, ct);

        if (!result.Success)
        {
            // Propagar error / validaciones recibidas
            return OperationResult.Fail<UpdateMedicalVisitModel>(result.Error!, result.ValidationErrors);
        }

        var patient = result.Value;
        if (patient is null)
        {
            return OperationResult.Unknown<UpdateMedicalVisitModel>("Paciente no encontrado.");
        }

        var model = new UpdateMedicalVisitModel
        {
            PatientId = patient.PatientId,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            HealthInsuranceName = patient.HealthInsuranceName,
            Acronym = patient.Acronym,
            HealthInsuranceCard = patient.HealthInsuranceCard,
            HealthInsuranceMemberNumber = patient.HealthInsuranceMemberNumber,
            HealthInsurancePlan = patient.HealthInsurancePlan
        };

        return OperationResult.Ok(model);
    }
}
