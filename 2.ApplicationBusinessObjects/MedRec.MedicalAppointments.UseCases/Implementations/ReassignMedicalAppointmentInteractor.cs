using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalAppointments.UseCases.Implementations;
internal class ReassignMedicalAppointmentInteractor : IReassignMedicalAppointmentInputPort
{
    private readonly IRepositoryUnitOfWork _unitOfWork;
    private readonly IReassignMedicalAppointmentOutputPort _presenter;
    private readonly IMedicalAppointmentCommandRepository _commandsRepository;
    private readonly IMedicalAppointmentQueriesRepository _queriesRepository;

    public ReassignMedicalAppointmentInteractor(
        IRepositoryUnitOfWork unitOfWork,
        IReassignMedicalAppointmentOutputPort presenter,
        IMedicalAppointmentCommandRepository commandsRepository,
        IMedicalAppointmentQueriesRepository queriesRepository)
    {
        _unitOfWork = unitOfWork;
        _presenter = presenter;
        _commandsRepository = commandsRepository;
        _queriesRepository = queriesRepository;
    }

    public async Task Handle(MedicalAppointmentDto reassignAppointmentDto, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
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
        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
           {
               await _commandsRepository.Reassign(entity, ct);
               await _unitOfWork.SaveChanges(ct);
               await _presenter.ErrorAsync(default);

               // Leer el turno actualizado (RowVersion y demás campos actuales)
               var reassigned = await _queriesRepository.GetById(reassignAppointmentDto.Id, ct);
               await _presenter.Handle(reassigned, ct);
           }, ct);
    }
}
