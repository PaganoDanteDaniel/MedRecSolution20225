using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class UpdateHealthInsuranceInteractor(
    IUpdateHealthInsuranceOutputPort presenter,
    IHealthInsuranceCommandRepository commandRepository,
    IRepositoryUnitOfWork unitOfWork) : IUpdateHealthInsuranceInputPort
{
    public async Task Handle(UpdateHealthInsuranceDto healthInsuranceDto, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Operación cancelada por el usuario.",
                ErrorCode.Cancelled,
                null,
                499));
            return;
        }
        try
        {
            if (healthInsuranceDto == null)
                throw new ArgumentNullException(nameof(healthInsuranceDto));
            var entity = new HealthInsuranceCompany()
            {
                Id = healthInsuranceDto.Id,
                Name = healthInsuranceDto.Name,
                Acronym = healthInsuranceDto.Acronym,
                RowVersion = healthInsuranceDto.RowVersion
            };

            await unitOfWork.ExecuteWithRetryAsync(async () =>
            {
                await unitOfWork.BeginTransaction(ct);
                try
                {
                    await commandRepository.Update(entity, ct);
                    await unitOfWork.SaveChanges(ct);
                    await unitOfWork.CommitTransaction(ct);
                }
                catch (Exception)
                {
                    await unitOfWork.RollbackTransaction(ct);
                    throw;
                }
            }, ct);

            await presenter.Handle(true, ct);
        }
        catch (ConcurrencyException cx)
        {
            // 409: incluir conflictos tipados (lista de ConcurrencyConflictDto)
            await presenter.ErrorAsync(new ErrorInfo(
                "Conflicto de concurrencia al crear la Obra Social.",
                ErrorCode.ConcurrencyError,
                cx.Conflicts,
                409));
        }
        catch (DuplicateKeyException dx)
        {
            // 409: conflicto por clave duplicada (Details suele contener entidades implicadas)
            await presenter.ErrorAsync(new ErrorInfo(
                "Ya existe un registro que viola una restricción de unicidad.",
                ErrorCode.DuplicateKey,
                dx.Details,
                409));
        }
        catch (UpdateException ux)
        {
            // 500: otros errores de persistencia
            await presenter.ErrorAsync(new ErrorInfo(
                "Error al persistir los cambios en la base de datos.",
                ErrorCode.UpdateError,
                ux.Details,
                500));
        }
        catch (BusinessException bx)
        {
            // Mantener compatibilidad con BusinessException si aparece desde otras capas
            await presenter.ErrorAsync(bx.Error);
        }
        catch (OperationCanceledException)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Operación cancelada por el usuario.",
                ErrorCode.Cancelled,
                null,
                499));
        }
        catch (Exception ex)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Ocurrió un error inesperado al crear la Obra Social.",
                ErrorCode.Unknown,
                new { Exception = ex.Message },
                500));
        }
    }
}
