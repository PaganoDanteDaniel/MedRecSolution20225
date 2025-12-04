using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Patients.UseCases.Implementations;

internal class CreatePatientInteractor : ICreatePatientInputPort
{
    private readonly ICreatePatientOutputPort _presenter;
    private readonly IPatientCommandsRepository _commandRepository;
    private readonly IModelValidatorHub<CreatePatientDto> _validatorHub;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public CreatePatientInteractor(
        ICreatePatientOutputPort presenter,
        IPatientCommandsRepository commandRepository,
        IModelValidatorHub<CreatePatientDto> validatorHub,
        IRepositoryUnitOfWork unitOfWork)
    {
        _presenter = presenter;
        _commandRepository = commandRepository;
        _validatorHub = validatorHub;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(CreatePatientDto dto, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var isValid = await _validatorHub.Validate(dto, p => CreatePatientValidator.Validate(p));
        if (!isValid)
        {
            await _presenter.ValidationErrorsAsync(_validatorHub.Errors);
            return;
        }

        var patient = (Patient)dto;

        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await _commandRepository.Create(patient, ct);
            await _unitOfWork.SaveChanges(ct);
            await _presenter.ErrorAsync(null);
        }, ct);

        await _presenter.ErrorAsync(null);
        await _presenter.Handle(ct);
    }
}