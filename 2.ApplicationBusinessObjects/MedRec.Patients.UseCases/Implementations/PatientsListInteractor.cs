using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Patients.UseCases.Implementations;

/// <summary>
/// Interactor para listar pacientes.
/// Mejora la confección de los ErrorInfo:
/// - Validaciones => ErrorCode.ValidationError + HTTP 400
/// - Cancelación => ErrorCode.Cancelled + HTTP 499
/// - No encontrado (0 registros) ya no se trata como error: se retorna lista vacía
/// - Lista nula => ErrorCode.DatabaseError + HTTP 500
/// - Excepciones inesperadas => ErrorCode.DatabaseError + detalles + HTTP 500
/// </summary>
internal class PatientsListInteractor : IPatientsListInputPort
{
    private readonly IPatientsListOutputPort _presenter;
    private readonly IPatientQueriesRepository _queriesRepository;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public PatientsListInteractor(
        IPatientsListOutputPort presenter,
        IPatientQueriesRepository queriesRepository,
        IRepositoryUnitOfWork unitOfWork)
    {
        _presenter = presenter;
        _queriesRepository = queriesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PaginationDto paginationDto, CancellationToken ct = default)
    {
        // Validación de parámetros de paginación
        if (paginationDto.CurrentPage < 1 || paginationDto.PageSize < 1)
        {
            await _presenter.ErrorAsync(new ErrorInfo(
                "La página y el tamaño deben ser mayores a cero.",
                ErrorCode.ValidationError,
                new { paginationDto.CurrentPage, paginationDto.PageSize },
                400));
            return;
        }

        await _unitOfWork.ExecuteWithRetry(async () =>
        {
            ct.ThrowIfCancellationRequested();

            // Total de registros (cero ya no es error)
            var totalRecords = await _queriesRepository.CountPatients(paginationDto.FilterOne, ct);

            // Si no hay registros, devolvemos lista vacía inmediatamente
            if (totalRecords == 0)
            {
                await _presenter.Handle(Enumerable.Empty<Patient>(), 0, ct);
                return;
            }

            ct.ThrowIfCancellationRequested();
            // Obtener la página solicitada
            var patients = await _queriesRepository.GetPatientsList(paginationDto, ct);

            if (patients is null)
            {
                await _presenter.ErrorAsync(new ErrorInfo(
                    "Error al obtener la lista de pacientes.",
                    ErrorCode.DatabaseError,
                    new { paginationDto.CurrentPage, paginationDto.PageSize, paginationDto.FilterOne },
                    500));
                return;
            }

            await _presenter.ErrorAsync(null);
            await _presenter.Handle(patients, totalRecords, ct);
        }, ct);
    }
}

