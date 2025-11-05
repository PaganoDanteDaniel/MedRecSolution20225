using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Exceptions;

namespace MedRec.MedicalAppointments.UseCases.Implementations;

internal class GetMedicalAppointmentsInteractor(
    IGetMedicalAppointmentsOutputPort presenter,
    IMedicalAppointmentQueriesRepository queriesRepository)
    : IGetMedicalAppointmentsInputPort
{
    public async Task Handle((DateTime startDate, DateTime endDate) rangeDate, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            // Delegar la obtención al repositorio de consultas
            var appointments = await queriesRepository.GetAllByDateRange(rangeDate, ct);

            // Entregar entidades al presenter (el presenter se encarga del mapeo a DTO)
            await presenter.Handle(appointments, ct);
        }
        catch (BusinessException bx)
        {
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
                "Ocurrió un error inesperado al obtener los turnos.",
                ErrorCode.Unknown,
                new { Exception = ex.Message },
                500));
        }
    }
}
