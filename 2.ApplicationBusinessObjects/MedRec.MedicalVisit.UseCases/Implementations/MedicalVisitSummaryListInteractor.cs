using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class MedicalVisitSummaryListInteractor : IMedicalVisitSummaryListInputPort
{
    private readonly IMedicalVisitSummaryListOutputPort _outputPort;
    private readonly IMedicalVisitQueriesRepositoryUoW _queriesRepository;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public MedicalVisitSummaryListInteractor(
        IMedicalVisitSummaryListOutputPort outputPort,
        IMedicalVisitQueriesRepositoryUoW queriesRepository,
        IRepositoryUnitOfWork unitOfWork)
    {
        _outputPort = outputPort;
        _queriesRepository = queriesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(Guid patientId, PaginationDto paginationDto = default, CancellationToken ct = default)
    {
        // 1. Validación de paginación (opcional pero recomendado)
        if (paginationDto is not null)
        {
            if (paginationDto.CurrentPage < 1)
            {
                await _outputPort.ErrorAsync(new ErrorInfo(
                    message: "El número de página debe ser mayor o igual a 1.",
                    code: ErrorCode.ValidationError,
                    httpStatusCode: 400
                ));
                return;
            }

            if (paginationDto.PageSize < 1 || paginationDto.PageSize > 100) // Límite razonable
            {
                await _outputPort.ErrorAsync(new ErrorInfo(
                    message: "El tamaño de página debe estar entre 1 y 100.",
                    code: ErrorCode.ValidationError,
                    httpStatusCode: 400
                ));
                return;
            }
        }

        ct.ThrowIfCancellationRequested();

        await _unitOfWork.ExecuteWithRetry(async () =>
        {
            // 2. Obtener visitas
            var visits = await _queriesRepository.GetMedicalVisits(patientId, paginationDto, ct);

            // 3. Entregar resultados (puede ser una lista vacía)
            await _outputPort.ErrorAsync(null);
            await _outputPort.Handle(visits ?? Enumerable.Empty<PatientMedicalVisit>());
        }, ct);
    }
}