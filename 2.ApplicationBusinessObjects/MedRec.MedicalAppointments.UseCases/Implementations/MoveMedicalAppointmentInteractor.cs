using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalAppointments.UseCases.Implementations;

internal class MoveMedicalAppointmentInteractor : IMoveMedicalAppointmentInputPort
{
    private readonly IRepositoryUnitOfWork _unitOfWork;
    private readonly IMoveMedicalAppointmentOutputPort _presenter;
    private readonly IMedicalAppointmentCommandRepository _commandsRepository;
    private readonly IMedicalAppointmentQueriesRepository _queriesRepository;

    public MoveMedicalAppointmentInteractor(
        IRepositoryUnitOfWork unitOfWork,
        IMoveMedicalAppointmentOutputPort presenter,
        IMedicalAppointmentCommandRepository commandsRepository,
        IMedicalAppointmentQueriesRepository queriesRepository)
    {
        _unitOfWork = unitOfWork;
        _presenter = presenter;
        _commandsRepository = commandsRepository;
        _queriesRepository = queriesRepository;
    }

    public async Task Handle(MoveMedicalAppointmentDto moveAppointmentDto, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            var entity = new MedicalAppointment
            {
                Id = moveAppointmentDto.Id,
                DateTime = moveAppointmentDto.DateTime,
                RowVersion = moveAppointmentDto.RowVersion
            };

            await _commandsRepository.Move(entity, ct);
            await _unitOfWork.SaveChanges(ct);
            await _presenter.ErrorAsync(default);
            // Leer el turno actualizado (RowVersion y demás campos actuales)
            var updated = await _queriesRepository.GetById(moveAppointmentDto.Id, ct);
            await _presenter.Handle(updated, ct);
        }, ct);
    }
}
