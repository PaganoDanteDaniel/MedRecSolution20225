using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class GetMedicalVisitInteractorUoW(
    IGetMedicalVisitOutputPort outputPort,
    IMedicalVisitQueriesRepositoryUoW queriesRepository) : IGetMedicalVisitInputPort
{
    public async Task Handle(Guid medicalVisitId, CancellationToken ct = default)
    {
        if (medicalVisitId == Guid.Empty)
        {
            await outputPort.ErrorAsync(new ErrorInfo(
                message: "El ID del paciente es inválido.",
                code: ErrorCode.ValidationError,
                httpStatusCode: 400
            ));
            return;
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            var result = await queriesRepository.GetMedicalVisit(medicalVisitId, ct).ConfigureAwait(false);
            await outputPort.Handle(result);
        }
        catch (OperationCanceledException)
        {
            // Cancelación intencionada: no es error de dominio
            // No se reporta al outputPort, se re-lanza para que capas superiores lo manejen
            throw;
        }
        catch (Exception ex)
        {
            // 5. Manejo de errores de base de datos o infraestructura
            await outputPort.ErrorAsync(new ErrorInfo(
                message: "Error al obtener la lista de visitas médicas.",
                code: ErrorCode.DatabaseError,
                details: new
                {
                    ExceptionType = ex.GetType().Name,
                    InnerMessage = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                },
                httpStatusCode: 500
            ));
        }
    }
}
