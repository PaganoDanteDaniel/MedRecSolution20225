using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Patients.UseCases.Implementations;

internal class UpdatePatientInteractor : IUpdatePatientInputPort
{
    private readonly IUpdatePatientOutputPort _presenter;
    private readonly IPatientCommandsRepository _commandsRepository;
    private readonly IModelValidatorHub<UpdatePatientDto> _validatorHub;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public UpdatePatientInteractor(
        IUpdatePatientOutputPort presenter,
        IPatientCommandsRepository commandsRepository,
        IModelValidatorHub<UpdatePatientDto> validatorHub,
        IRepositoryUnitOfWork unitOfWork)
    {
        _presenter = presenter;
        _commandsRepository = commandsRepository;
        _validatorHub = validatorHub;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePatientDto editPatient, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var esValido = await _validatorHub.Validate(editPatient, p => UpdatePatientValidator.Validate(p, true));
        if (!esValido)
        {
            await _presenter.ValidationErrorsAsync(_validatorHub.Errors);
            return;
        }

        var patient = (Patient)editPatient;

        // Sólo manejamos rollback transaccional interno; las excepciones se propagan al proxy.
        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await _commandsRepository.Update(patient, ct);
            await _unitOfWork.SaveChanges(ct);
            await _presenter.ErrorAsync(null);
            await _presenter.Handle(ct);
        }, ct);
    }
}

