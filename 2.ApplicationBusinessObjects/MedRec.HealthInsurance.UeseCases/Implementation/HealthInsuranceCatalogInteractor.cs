using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class HealthInsuranceCatalogInteractor(
    IHealthInsuranceCatalogOutputPort presenter,
    IHealthInsuranceQueriesRepository queriesRepository) : IHealthInsuranceCatalogInputPort
{
    public async Task Handle(PaginationDto pagination, CancellationToken ct = default)
    {
        // 1. Validación de paginación
        if (pagination is not null)
        {
            if (pagination.CurrentPage < 1)
            {
                await presenter.ErrorAsync(new ErrorInfo(
                    "El número de página debe ser mayor o igual a 1.",
                    ErrorCode.ValidationError,
                    httpStatusCode: 400));
                return;
            }

            if (pagination.PageSize < 1 || pagination.PageSize > 100)
            {
                await presenter.ErrorAsync(new ErrorInfo(
                    "El tamaño de página debe estar entre 1 y 100.",
                    ErrorCode.ValidationError,
                    httpStatusCode: 400));
                return;
            }
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            int totalCount = await queriesRepository.GetCount(pagination?.FilterOne, ct);
            var result = await queriesRepository.GetAll(pagination, ct);

            await presenter.Handle(result, totalCount, ct);
        }
        catch (BusinessException bx)
        {
            // Errores de negocio (ej: filtro inválido, etc.)
            await presenter.ErrorAsync(bx.Error);
        }
        catch (OperationCanceledException)
        {
            // Cancelación intencionada: re-lanzar
            throw;
        }
        catch (Exception ex)
        {
            // Error inesperado de infraestructura
            await presenter.ErrorAsync(new ErrorInfo(
                "Ocurrió un error inesperado al obtener el catálogo de obras sociales.",
                ErrorCode.Unknown,
                new { Exception = ex.Message },
                500));
        }
    }
}
