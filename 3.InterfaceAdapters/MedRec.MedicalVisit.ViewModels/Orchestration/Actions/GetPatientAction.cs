using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions;
public class GetPatientAction(
    IPatientForMedicalVisitInputPort inPort,
    IPatientForMedicalVisitOutputPort outPort) : IGetPatientAction
{
    public async Task<OperationResult<CreateMedicalVisitModel>> ExecuteAsync(Guid patientId, CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            await inPort.Handle(patientId, ct);

            if (outPort.ErrorMessage is not null || outPort.ValidationErrors.Any())
                return OperationResult.Fail<CreateMedicalVisitModel>(outPort.ErrorMessage, outPort.ValidationErrors);

            if (outPort.DataPatient is null)
                return OperationResult.Unknown<CreateMedicalVisitModel>();

            var model = MedicalVisitMapper.ToCreateModel(outPort.DataPatient);

            return OperationResult.Ok(model);

        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<CreateMedicalVisitModel>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<CreateMedicalVisitModel>(
                new ErrorInfo($"Error crítico al obtener la historia clínica del paciente: {ex.Message}"));
        }
    }
}
