using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.HealthInsurance.BusinessObjects.Validators;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;
using MedRec.Validator.Interfaces;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class CreateHealthInsuranceInteractor(
    ICreateHealthInsuranceOutputPort presenter,
    IHealthInsuranceCommandRepository commandRepository,
    IModelValidatorHub<CreateHealthInsuranceDto> validatorHub,
    IRepositoryUnitOfWork unitOfWork) : ICreateHealthInsuranceInputPort
{
    public async Task Handle(CreateHealthInsuranceDto healthCompany, CancellationToken ct = default)
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
            if (healthCompany == null)
                throw new ArgumentNullException(nameof(healthCompany));

            bool isValid = await validatorHub.Validate(healthCompany,
                h => CreateHealthInsuranceValidator.Validate(h));

            if (!isValid)
            {
                await presenter.ValidationErrorsAsync(validatorHub.Errors);
                return;
            }

            var entity = new HealthInsuranceCompany()
            {
                Name = healthCompany.Name,
                Acronym = healthCompany.Acronym
            };

            await unitOfWork.ExecuteWithRetry(async () =>
            {
                await unitOfWork.BeginTransaction(ct);
                try
                {
                    await commandRepository.Create(entity, ct);
                    await unitOfWork.SaveChanges(ct); // GuardDBContext traduce excepciones SQL a las nuestras
                    await unitOfWork.CommitTransaction(ct);
                }
                catch
                {
                    await unitOfWork.RollbackTransaction(ct);
                    throw;
                }
            }, ct);

            await presenter.Handle(entity, ct);
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
