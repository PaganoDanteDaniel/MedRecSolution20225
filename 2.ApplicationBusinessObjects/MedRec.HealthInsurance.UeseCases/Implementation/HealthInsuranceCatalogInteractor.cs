using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class HealthInsuranceCatalogInteractor : IHealthInsuranceCatalogInputPort
{

    private readonly IHealthInsuranceCatalogOutputPort _presenter;
    private readonly IHealthInsuranceQueriesRepository _queriesRepository;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public HealthInsuranceCatalogInteractor(
        IHealthInsuranceCatalogOutputPort presenter,
        IHealthInsuranceQueriesRepository queriesRepository,
        IRepositoryUnitOfWork unitOfWork)
    {
        _presenter = presenter;
        _queriesRepository = queriesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PaginationDto pagination, CancellationToken ct = default)
    {
        // Normalizar parámetros y aplicar defaults si pagination es null
        var currentPage = pagination?.CurrentPage ?? 1;
        var pageSize = pagination?.PageSize ?? 10;

        // Mismo filtro para count y page: trim y null si queda vacío
        var filter = string.IsNullOrWhiteSpace(pagination?.FilterOne)
            ? null
            : pagination!.FilterOne.Trim();

        // Validaciones
        if (currentPage < 1)
        {
            await _presenter.ErrorAsync(new ErrorInfo(
                "El número de página debe ser mayor o igual a 1.",
                ErrorCode.ValidationError,
                httpStatusCode: 400));
            return;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            await _presenter.ErrorAsync(new ErrorInfo(
                "El tamaño de página debe estar entre 1 y 100.",
                ErrorCode.ValidationError,
                httpStatusCode: 400));
            return;
        }

        // Construir dto consistente para el repositorio
        var effectivePagination = new PaginationDto(currentPage, pageSize, filter);

        ct.ThrowIfCancellationRequested();

        await _unitOfWork.ExecuteWithRetry(async () =>
        {
            ct.ThrowIfCancellationRequested();

            // Total con el mismo filtro normalizado
            var totalCount = await _queriesRepository.GetCount(filter, ct);

            // Página con dto consistente
            var result = await _queriesRepository.GetAll(effectivePagination, ct);

            await _presenter.Handle(result, totalCount, ct);
        }, ct);
    }
}
