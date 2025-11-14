using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace MedRec.MedicalAppointments.UseCases.Implementations;

internal class CreateMedicalAppointmentInteractor(
    ICreateMedicalAppointmentOutputPort presenter,
    IRepositoryUnitOfWork unitOfWork,
    IMedicalAppointmentCommandRepository commandRepository,
    IMedicalAppointmentQueriesRepository queriesRepository)
    : ICreateMedicalAppointmentInputPort
{
    public async Task Handle(CreateMedicalAppointmentDto createAppointmentDto, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entity = ToEntity(createAppointmentDto);

        try
        {
            await unitOfWork.ExecuteWithRetryAsync(async () =>
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

            // Notificar resultado solo si la transacción se confirmó
            var created = await queriesRepository.GetById(entity.Id, ct);

            await presenter.Handle(created, ct);
        }
        catch (LostConnectionException lce)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                lce.Message,
                ErrorCode.DatabaseError,
                503));
        }
        catch (ConcurrencyException cx)
        {
            // 409: incluir conflictos tipados (lista de ConcurrencyConflictDto)
            await presenter.ErrorAsync(new ErrorInfo(
                "Conflicto de concurrencia al crear el turno.",
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
            throw;
        }
        catch (Exception ex)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "Ocurrió un error inesperado al crear el turno.",
                ErrorCode.Unknown,
                new { Exception = ex.Message },
                500));
        }
    }

    private static MedicalAppointment ToEntity(CreateMedicalAppointmentDto dto) =>
        new()
        {

            DateTime = dto.DateTime,
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Reason = dto.Reason
        };
}