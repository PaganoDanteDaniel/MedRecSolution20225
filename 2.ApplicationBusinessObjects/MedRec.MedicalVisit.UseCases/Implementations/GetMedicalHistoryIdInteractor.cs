using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class GetMedicalHistoryIdInteractor(
    IGetMedicalHistoryIdOutputPort _outputPort,
    IMedicalVisitQueriesRepository _repository,
    IMedicalVisitCommandRepository _command) : IGetMedicalHistoryIdInputPort
{
    public async Task Handle(Guid patientId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        var result = await _repository.GetMedicalHistory(patientId, cts);

        if (!result.IsSuccess)
        {
            if (result.Error.Code == ErrorCode.NotFound)
            {
                var medHist = await _command.CreateMedicalHistory(patientId, cts);
                if (!medHist.IsSuccess)
                {
                    await _outputPort.ErrorAsync(medHist.Error);
                    return;
                }

                await _outputPort.Handle(medHist.Value, cts);
            }
            else
            {
                await _outputPort.ErrorAsync(new ErrorInfo("No se pudo obtenre la historia clínica del paciente.", ErrorCode.DatabaseError));
                return;
            }
        }

    }
}
