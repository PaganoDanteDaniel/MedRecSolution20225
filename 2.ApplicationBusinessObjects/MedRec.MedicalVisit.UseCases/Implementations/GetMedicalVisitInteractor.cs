using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class GetMedicalVisitInteractor : IGetMedicalVisitInputPort
{
    private readonly IGetMedicalVisitOutputPort _outputPort;
    private readonly IMedicalVisitQueriesRepositoryUoW _queriesRepository;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public GetMedicalVisitInteractor(
        IGetMedicalVisitOutputPort outputPort,
        IMedicalVisitQueriesRepositoryUoW queriesRepository,
        IRepositoryUnitOfWork unitOfWork)
    {
        _outputPort = outputPort;
        _queriesRepository = queriesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(Guid medicalVisitId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await _unitOfWork.ExecuteWithRetry(async () =>
        {
            var result = await _queriesRepository.GetMedicalVisit(medicalVisitId, ct);
            if (result is null)
            {
                await _outputPort.ErrorAsync(new ErrorInfo(
                    $"La historia clínica no existe o fue eliminada.",
                    ErrorCode.NotFound,
                    new { MedicalVisitId = medicalVisitId },
                    404));
                return;
            }
            await _outputPort.ErrorAsync(null);
            await _outputPort.Handle(result, ct);
        }, ct);
    }
}
