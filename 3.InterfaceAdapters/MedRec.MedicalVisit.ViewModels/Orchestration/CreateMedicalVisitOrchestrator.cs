using MedRec.BusinessObjects.Results;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.MedicalVisit.ViewModels.Orchestration.Interfaces;

namespace MedRec.MedicalVisit.ViewModels.Orchestration;
internal class CreateMedicalVisitOrchestrator(
    IGetPatientAction getPatientAction,
    IGetMedicalHistoryAction getMedicalHistory,
    ICreateMedicalVisitAction createMedicalVisit) : ICreateMedicalVisitOrchestrator
{
    public async Task<OperationResult<bool>> CreateMedicalVisit(CreateMedicalVisitModel model, CancellationToken ct = default) =>
       await createMedicalVisit.ExecuteAsync(model, ct);

    public async Task<OperationResult<Guid>> GetHistoryId(Guid id, CancellationToken ct) =>
       await getMedicalHistory.ExecuteAsync(id, ct);
    public async Task<OperationResult<CreateMedicalVisitModel>> GetPatient(Guid id, CancellationToken ct)
    {
        var result = await getPatientAction.ExecuteAsync(id, ct);

        if (!result.Success)
        {
            // Propagar error / validaciones recibidas
            return OperationResult.Fail<CreateMedicalVisitModel>(result.Error!, result.ValidationErrors);
        }

        var patient = result.Value;
        if (patient is null)
        {
            return OperationResult.Unknown<CreateMedicalVisitModel>("Paciente no encontrado.");
        }

        var model = new CreateMedicalVisitModel
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
