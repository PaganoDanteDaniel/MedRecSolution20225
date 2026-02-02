using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class MedicalVisitSummaryListInteractorUoW(
    IMedicalVisitSummaryListOutputPort outputPort,
    IMedicalVisitQueriesRepositoryUoW queriesRepository) : IMedicalVisitSummaryListInputPort
{
    public async Task Handle(Guid patientId, PaginationDto paginationDto = default, CancellationToken ct = default)
    {
        // 1. Validación de entrada
        if (patientId == Guid.Empty)
        {
            await outputPort.ErrorAsync(new ErrorInfo(
                message: "El ID del paciente es inválido.",
                code: ErrorCode.ValidationError,
                httpStatusCode: 400
            ));
            return;
        }

        // 2. Validación de paginación (opcional pero recomendado)
        if (paginationDto is not null)
        {
            if (paginationDto.CurrentPage < 1)
            {
                await outputPort.ErrorAsync(new ErrorInfo(
                    message: "El número de página debe ser mayor o igual a 1.",
                    code: ErrorCode.ValidationError,
                    httpStatusCode: 400
                ));
                return;
            }

            if (paginationDto.PageSize < 1 || paginationDto.PageSize > 100) // Límite razonable
            {
                await outputPort.ErrorAsync(new ErrorInfo(
                    message: "El tamaño de página debe estar entre 1 y 100.",
                    code: ErrorCode.ValidationError,
                    httpStatusCode: 400
                ));
                return;
            }
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            // 3. Obtener visitas
            var visits = await queriesRepository.GetMedicalVisits(patientId, paginationDto, ct);

            // 4. Entregar resultados (puede ser una lista vacía)
            await outputPort.Handle(visits ?? Enumerable.Empty<PatientMedicalVisit>());
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