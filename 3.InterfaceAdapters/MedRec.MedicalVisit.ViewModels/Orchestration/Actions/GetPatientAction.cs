using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.ViewModels.Models;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalVisit.ViewModels.Orchestration.Actions;
public class GetPatientAction(
    IPatientForMedicalVisitInputPort inPort,
    IPatientForMedicalVisitOutputPort outPort) : IGetPatientAction
{
    public async Task<OperationResult<PatientModel>> ExecuteAsync(Guid patientId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            await inPort.Handle(patientId, ct);
            var result = outPort.Result;

            if (!result.Success)
            {
                if (result.Error is not null)
                    return OperationResult.Fail<PatientModel>(result.Error, null);
                return OperationResult.Unknown<PatientModel>("Estado de salida inconsistente");
            }

            if (result.Value is null)
                return OperationResult.Unknown<PatientModel>("Paciente no encontrado");

            var model = MedicalVisitMapper.ToPatientModel(result.Value);

            return OperationResult.Ok(model);

        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<PatientModel>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<PatientModel>(
                new ErrorInfo($"Error crítico al obtener datos del paciente: {ex.Message}", ErrorCode.Unknown, new { patientId }, 500), null);
        }
    }
}
