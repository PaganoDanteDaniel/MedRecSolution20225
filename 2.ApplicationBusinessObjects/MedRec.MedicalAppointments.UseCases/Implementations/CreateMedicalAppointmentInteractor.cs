using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalAppointments.UseCases.Implementations;

internal class CreateMedicalAppointmentInteractor
    : ICreateMedicalAppointmentInputPort
{
    private readonly ICreateMedicalAppointmentOutputPort _presenter;
    private readonly IRepositoryUnitOfWork _unitOfWork;
    private readonly IMedicalAppointmentCommandRepository _commandRepository;
    private readonly IMedicalAppointmentQueriesRepository _queriesRepository;

    public CreateMedicalAppointmentInteractor(
        ICreateMedicalAppointmentOutputPort presenter,
        IRepositoryUnitOfWork unitOfWork,
        IMedicalAppointmentCommandRepository commandRepository,
        IMedicalAppointmentQueriesRepository queriesRepository)
    {
        _presenter = presenter;
        _unitOfWork = unitOfWork;
        _commandRepository = commandRepository;
        _queriesRepository = queriesRepository;
    }

    public async Task Handle(CreateMedicalAppointmentDto createAppointmentDto, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entity = ToEntity(createAppointmentDto);


        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await _commandRepository.Create(entity, ct);
            await _unitOfWork.SaveChanges(ct);
            await _presenter.ErrorAsync(null);

            // Notificar resultado solo si la transacción se confirmó
            var created = await _queriesRepository.GetById(entity.Id, ct);
            await _presenter.Handle(created, ct);
        }, ct);

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