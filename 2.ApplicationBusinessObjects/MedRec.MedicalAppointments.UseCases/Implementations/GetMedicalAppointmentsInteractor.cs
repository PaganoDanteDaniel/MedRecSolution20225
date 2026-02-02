using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalAppointments.UseCases.Implementations;

internal class GetMedicalAppointmentsInteractor : IGetMedicalAppointmentsInputPort
{
    private readonly IGetMedicalAppointmentsOutputPort _presenter;
    private readonly IMedicalAppointmentQueriesRepository _queriesRepository;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public GetMedicalAppointmentsInteractor(
        IGetMedicalAppointmentsOutputPort presenter,
        IMedicalAppointmentQueriesRepository queriesRepository,
        IRepositoryUnitOfWork unitOfWork)
    {
        _presenter = presenter;
        _queriesRepository = queriesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle((DateTime startDate, DateTime endDate) rangeDate, CancellationToken ct)
    {
        // 1. Validación de rango de fechas
        if (rangeDate.startDate > rangeDate.endDate)
        {
            var aux = rangeDate.startDate;
            rangeDate.startDate = rangeDate.endDate;
            rangeDate.endDate = aux;
        }

        // 2. Validación de rango razonable (opcional pero recomendado)
        var maxRange = TimeSpan.FromDays(7); // Máximo 1 semana
        if (rangeDate.endDate - rangeDate.startDate > maxRange)
        {
            await _presenter.ErrorAsync(new ErrorInfo(
                "El rango de fechas no puede superar 7 días.",
                ErrorCode.ValidationError,
                httpStatusCode: 400));
            return;
        }

        ct.ThrowIfCancellationRequested();


        await _unitOfWork.ExecuteWithRetry(async () =>
        {
            var appointments = await _queriesRepository.GetAllByDateRange(rangeDate, ct);
            await _presenter.ErrorAsync(default);
            await _presenter.Handle(appointments, ct);
        }, ct);
    }
}
