using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MedRec.HealthInsurance.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.HealthInsurance.UseCases.Implementation;
internal class CreateHealthInsuranceInteractor : ICreateHealthInsuranceInputPort
{
    private readonly ICreateHealthInsuranceOutputPort _presenter;
    private readonly IHealthInsuranceCommandRepository _commandRepository;
    private readonly IModelValidatorHub<CreateHealthInsuranceDto> _validatorHub;
    private readonly IRepositoryUnitOfWork _unitOfWork;
    public CreateHealthInsuranceInteractor(
    ICreateHealthInsuranceOutputPort presenter,
    IHealthInsuranceCommandRepository commandRepository,
    IModelValidatorHub<CreateHealthInsuranceDto> validatorHub,
    IRepositoryUnitOfWork unitOfWork)
    {
        _presenter = presenter;
        _commandRepository = commandRepository;
        _validatorHub = validatorHub;
        _unitOfWork = unitOfWork;
    }
    public async Task Handle(CreateHealthInsuranceDto healthCompany, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        bool isValid = await _validatorHub.Validate(healthCompany,
            h => CreateHealthInsuranceValidator.Validate(h));

        if (!isValid)
        {
            await _presenter.ValidationErrorsAsync(_validatorHub.Errors);
            return;
        }

        var entity = new HealthInsuranceCompany()
        {
            Name = healthCompany.Name,
            Acronym = healthCompany.Acronym
        };

        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await _commandRepository.Create(entity, ct);
            await _unitOfWork.SaveChanges(ct);
            await _presenter.ErrorAsync(null);

        }, ct);

        await _presenter.Handle(ct);
    }
}
