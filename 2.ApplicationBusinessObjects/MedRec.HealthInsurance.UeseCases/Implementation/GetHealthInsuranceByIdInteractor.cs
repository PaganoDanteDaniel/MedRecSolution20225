using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class GetHealthInsuranceByIdInteractor : IGetHealthInsuranceByIdInputPort
{

    private readonly IGetHealthInsuranceByIdOutputPort _presenter;
    private readonly IHealthInsuranceQueriesRepository _queriesRepository;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public GetHealthInsuranceByIdInteractor(IGetHealthInsuranceByIdOutputPort presenter,
        IHealthInsuranceQueriesRepository queriesRepository,
        IRepositoryUnitOfWork unitOfWork)
    {
        _presenter = presenter;
        _queriesRepository = queriesRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await _unitOfWork.ExecuteWithRetry(async () =>
        {
            if (id == Guid.Empty)
            {
                await _presenter.ErrorAsync(new ErrorInfo(
                    "El ID de la obra social es inválido.",
                    ErrorCode.ValidationError,
                    httpStatusCode: 400));
                return;
            }
            var entity = await _queriesRepository.GetById(id, ct);
            await _presenter.Handle(entity, ct);
        }, ct);
    }
}
