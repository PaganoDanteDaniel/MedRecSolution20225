using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.BusinessObjects.Validator;
using MedRec.Validator.Interfaces;

namespace MedRec.MedicalVisit.UseCases.Implementations;
public class UpdateMedicalVisitInteractor : IUpdateMedicalVisitInputPort
{
    private readonly IUpdateMedicalVisitOutputPort _outputPort;
    private readonly IMedicalVisitCommandRepositoryUoW _commandRepository;
    private readonly IMedicalVisitQueriesRepositoryUoW _queriesRepository;
    private readonly IModelValidatorHub<UpdateMedicalVisitDto> _validatorHub;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public UpdateMedicalVisitInteractor(
        IUpdateMedicalVisitOutputPort outputPort,
        IMedicalVisitCommandRepositoryUoW commandRepository,
        IMedicalVisitQueriesRepositoryUoW queriesRepository,
        IModelValidatorHub<UpdateMedicalVisitDto> validatorHub,
        IRepositoryUnitOfWork unitOfWork)
    {
        _outputPort = outputPort;
        _commandRepository = commandRepository;
        _queriesRepository = queriesRepository;
        _validatorHub = validatorHub;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateMedicalVisitDto dto, CancellationToken ct = default)
    {
        if (!await _validatorHub.Validate(dto, v => UpdateMedicalVisitValidator.Validate(v)))
        {
            await _outputPort.ValidationErrorsAsync(_validatorHub.Errors);
            return;
        }

        ct.ThrowIfCancellationRequested();

        await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            var updateMedicalVisit = (PatientMedicalVisit)dto;

            await _commandRepository.Update(updateMedicalVisit, ct);
            var response = await _unitOfWork.SaveChanges(ct);
            await _outputPort.ErrorAsync(null);
            await _outputPort.Handle(response > 0, ct);

        }, ct);

    }
}
