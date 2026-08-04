using MedRec.Entity.Interfaces;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.Shared.Gruards;

namespace MedRec.MedicalVisit.UseCases.Implementations;
public class GetMedicalHistoryIdInteractor : IGetMedicalHistoryIdInputPort
{
    private readonly IGetMedicalHistoryIdOutputPort _outputPort;
    private readonly IMedicalVisitQueriesRepositoryUoW _repository;
    private readonly IMedicalVisitCommandRepositoryUoW _command;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public GetMedicalHistoryIdInteractor(
        IGetMedicalHistoryIdOutputPort outputPort,
        IMedicalVisitQueriesRepositoryUoW repository,
        IMedicalVisitCommandRepositoryUoW command,
        IRepositoryUnitOfWork unitOfWork)
    {
        _outputPort = outputPort;
        _repository = repository;
        _command = command;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(Guid patientId, CancellationToken ct = default)
    {
        var guard = Guard.Against(patientId, nameof(patientId)).NotNullOrEmpty();
        if (!guard.IsValid)
        {
            await _outputPort.ValidationErrorsAsync(guard.Errors);
            return;
        }

        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            // 1. Intentar obtener historial existente
            Guid existingId = await _repository.GetMedicalHistory(patientId, ct);
            if (existingId != Guid.Empty)
            {
                await _outputPort.Handle(existingId, ct);
                return;
            }

            // 2. No existe → crear dentro de transacción

            Guid newId = await _command.CreateMedicalHistory(patientId, ct);
            await _unitOfWork.SaveChanges(ct);

            await _outputPort.ErrorAsync(null);
            await _outputPort.Handle(newId, ct);
        }, ct);
    }
}
