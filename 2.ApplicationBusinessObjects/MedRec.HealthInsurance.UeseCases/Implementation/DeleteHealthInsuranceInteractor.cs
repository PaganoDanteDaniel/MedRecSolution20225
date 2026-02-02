using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class DeleteHealthInsuranceInteractor : IDeleteHealthInsuranceInputPort
{
    private readonly IHealthInsuranceCommandRepository _commandRepository;
    private readonly IHealthInsuranceQueriesRepository _queriesRepository;
    private readonly IDeleteHealthInsuranceOutputPort _presenter;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public DeleteHealthInsuranceInteractor(
        IHealthInsuranceCommandRepository commandRepository,
        IHealthInsuranceQueriesRepository queriesRepository,
        IDeleteHealthInsuranceOutputPort presenter,
        IRepositoryUnitOfWork unitOfWork)
    {
        _commandRepository = commandRepository;
        _queriesRepository = queriesRepository;
        _presenter = presenter;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(Guid Id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            var entity = await _queriesRepository.GetById(Id, ct);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await _commandRepository.SoftDelete(entity, ct);
                await _unitOfWork.SaveChanges();
                await _presenter.ErrorAsync(null);
            }
            else
            {
                await _presenter.ErrorAsync(
                    new ErrorInfo("El registro no existe o ya fue eliminado por otro usuario", ErrorCode.NotFound));
            }

        }, ct);

        await _presenter.Handle(ct);
    }
}
