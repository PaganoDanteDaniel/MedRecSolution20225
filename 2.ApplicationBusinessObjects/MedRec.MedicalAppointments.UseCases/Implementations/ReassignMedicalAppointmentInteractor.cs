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
internal class ReassignMedicalAppointmentInteractor(
    IRepositoryUnitOfWork unitOfWork,
    IReassignMedicalAppointmentOutputPort presenter,
    IMedicalAppointmentCommandRepository commandsRepository,
    IMedicalAppointmentQueriesRepository queriesRepository) : IReassignMedicalAppointmentInputPort
{
    public async Task Handle(MedicalAppointmentDto reassignAppointmentDto, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            await unitOfWork.ExecuteWithRetry(async () =>
            {
                await unitOfWork.BeginTransaction(ct);
                try
                {
                    // Construye la entidad mínima requerida por el comando Move (Id, AppointmentDateTime, RowVersion)
                    var entity = new MedicalAppointment
                    {
                        Id = reassignAppointmentDto.Id,
                        DateTime = reassignAppointmentDto.DateTime,
                        PatientId = reassignAppointmentDto.PatientId,
                        DoctorId = reassignAppointmentDto.DoctorId,
                        Reason = reassignAppointmentDto.Reason,
                        RowVersion = reassignAppointmentDto.RowVersion
                    };

                    await commandsRepository.Reassign(entity, ct);
                    await unitOfWork.SaveChanges(ct);
                    await unitOfWork.CommitTransaction(ct);
                }
                catch
                {
                    await unitOfWork.RollbackTransaction(ct);
                    throw;
                }
            }, ct);

            // Leer el turno actualizado (RowVersion y demás campos actuales)
            var reassigned = await queriesRepository.GetById(reassignAppointmentDto.Id, ct);
            await presenter.Handle(reassigned, ct);
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
            await presenter.ErrorAsync(new ErrorInfo(
                "EL TURNO FUE MODIFICADO POR OTRO USUARIO",
                ErrorCode.ConcurrencyError,
                cx.Conflicts,
                409));
        }
        catch (DuplicateKeyException dx)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "NO SE PUDO MOVER EL TURNO PORQUE YA EXISTE UNO",
                ErrorCode.DuplicateKey,
                dx.Details,
                409));
        }
        catch (UpdateException ux)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "ERROR AL INTENTAR GUARDAR EL TURNO",
                ErrorCode.UpdateError,
                ux.Details,
                500));
        }
        catch (BusinessException bx)
        {
            await presenter.ErrorAsync(bx.Error);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "OCURRIO UN ERROR INESPERADO AL MOVER EL TURNO",
                ErrorCode.Unknown,
                new { Exception = ex.Message },
                500));
        }
    }
}
