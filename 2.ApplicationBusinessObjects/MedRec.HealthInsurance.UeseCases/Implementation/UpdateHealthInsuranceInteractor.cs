using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.HealthInsurance.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class UpdateHealthInsuranceInteractor : IUpdateHealthInsuranceInputPort
{
    private readonly IUpdateHealthInsuranceOutputPort _presenter;
    private readonly IHealthInsuranceCommandRepository _commandRepository;
    private readonly IRepositoryUnitOfWork _unitOfWork;
    private readonly IModelValidatorHub<UpdateHealthInsuranceDto> _validatorHub;

    public UpdateHealthInsuranceInteractor(
        IUpdateHealthInsuranceOutputPort presenter,
        IHealthInsuranceCommandRepository commandRepository,
        IRepositoryUnitOfWork unitOfWork,
        IModelValidatorHub<UpdateHealthInsuranceDto> validatorHub)
    {
        _presenter = presenter;
        _commandRepository = commandRepository;
        _unitOfWork = unitOfWork;
        _validatorHub = validatorHub;
    }

    public async Task Handle(UpdateHealthInsuranceDto healthInsuranceDto, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            await _presenter.ErrorAsync(new ErrorInfo(
                "Operación cancelada por el usuario.",
                ErrorCode.Cancelled,
                null,
                499));
            return;
        }

        bool isValid = await _validatorHub.Validate(healthInsuranceDto,
            h => UpdateHealthInsuranceValidator.Validate(h));

        if (!isValid)
        {
            await _presenter.ValidationErrorsAsync(_validatorHub.Errors);
            return;
        }

        var entity = new HealthInsuranceCompany()
        {
            Id = healthInsuranceDto.Id,
            Name = healthInsuranceDto.Name,
            Acronym = healthInsuranceDto.Acronym,
            RowVersion = healthInsuranceDto.RowVersion
        };

        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await _commandRepository.Update(entity, ct);
            await _unitOfWork.SaveChanges(ct);
            await _presenter.ErrorAsync(null);

        }, ct);

        await _presenter.Handle(ct);
    }
}
