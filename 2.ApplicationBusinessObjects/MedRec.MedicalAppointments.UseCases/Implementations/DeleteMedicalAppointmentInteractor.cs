using MedRec.Entity.Interfaces;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalAppointments.UseCases.Implementations;
internal class DeleteMedicalAppointmentInteractor(
    IDeleteMedicalAppointmentOutputPort presenter,
    IRepositoryUnitOfWork unitOfWork,
    IMedicalAppointmentCommandRepository commandRepository) : IDeleteMedicalAppointmentInputPort
{
    public async Task Handle(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        bool deleted = false;

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await commandRepository.Delete(id, ct);

            var affected = await unitOfWork.SaveChanges(ct);

            deleted = affected > 0; // idempotente: false si no existía

            await presenter.ErrorAsync(default);
            await presenter.Handle(deleted, ct);
        }, ct);
    }
}