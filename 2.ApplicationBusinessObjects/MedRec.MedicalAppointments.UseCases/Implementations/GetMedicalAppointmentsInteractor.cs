using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace MedRec.MedicalAppointments.UseCases.Implementations;

internal class GetMedicalAppointmentsInteractor(
    IGetMedicalAppointmentsOutputPort presenter,
    IMedicalAppointmentQueriesRepository queriesRepository)
    : IGetMedicalAppointmentsInputPort
{
    public async Task Handle((DateTime startDate, DateTime endDate) rangeDate, CancellationToken ct)
    {
        // 1. Validación de rango de fechas
        if (rangeDate.startDate > rangeDate.endDate)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "La fecha de inicio no puede ser posterior a la fecha de fin.",
                ErrorCode.ValidationError,
                httpStatusCode: 400));
            return;
        }

        // 2. Validación de rango razonable (opcional pero recomendado)
        var maxRange = TimeSpan.FromDays(365); // Máximo 1 año
        if (rangeDate.endDate - rangeDate.startDate > maxRange)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                "El rango de fechas no puede superar 365 días.",
                ErrorCode.ValidationError,
                httpStatusCode: 400));
            return;
        }

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
            var appointments = await queriesRepository.GetAllByDateRange(rangeDate, ct);
            await presenter.Handle(appointments, ct);
        }
        catch (LostConnectionException lce)
        {
            await presenter.ErrorAsync(new ErrorInfo(
                lce.Message,
                ErrorCode.DatabaseError,
                503));
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
                "Ocurrió un error inesperado al obtener los turnos.",
                ErrorCode.Unknown,
                new { Exception = ex.Message },
                500));
        }
    }
}
