using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.DynamicTemplates.BusinessObjects.Validators;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Validator.Interfaces;

namespace MedRec.DynamicTemplates.UseCases.Implementations;

/// <summary>
/// Interactor for SaveDynamicFields use case
/// </summary>
internal class SaveDynamicFieldsInteractor : ISaveDynamicFieldsInputPort
{
    private readonly ISaveDynamicFieldsOutputPort _outputPort;
    private readonly IMedicalVisitDynamicFieldCommandRepositoryUoW _commandRepository;
    private readonly IMedicalVisitDynamicFieldQueriesRepositoryUoW _queriesRepository;
    private readonly IModelValidatorHub<SaveDynamicFieldsDto> _validatorHub;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public SaveDynamicFieldsInteractor(
        ISaveDynamicFieldsOutputPort outputPort,
        IMedicalVisitDynamicFieldCommandRepositoryUoW commandRepository,
        IMedicalVisitDynamicFieldQueriesRepositoryUoW queriesRepository,
        IModelValidatorHub<SaveDynamicFieldsDto> validatorHub,
        IRepositoryUnitOfWork unitOfWork)
    {
        _outputPort = outputPort;
        _commandRepository = commandRepository;
        _queriesRepository = queriesRepository;
        _validatorHub = validatorHub;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SaveDynamicFieldsDto dto, CancellationToken cts = default)
    {
        try
        {
            cts.ThrowIfCancellationRequested();

            // Validación
            var isValid = await _validatorHub.Validate(dto, d => SaveDynamicFieldsValidator.Validate(d));
            if (!isValid)
            {
                var errors = _validatorHub.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToList()
                    );

                await _outputPort.HandleValidationErrors(errors);
                return;
            }

            await _unitOfWork.ExecuteInTransactionWithRetry(async () =>
            {
                // Eliminar campos existentes
                var existingFields = await _queriesRepository.GetByVisitId(dto.PatientMedicalVisitId, cts);
                foreach (var field in existingFields)
                {
                    await _commandRepository.DeleteByVisitId(field.Id, cts);
                }

                // Crear nuevos campos
                int savedCount = 0;
                foreach (var fieldValue in dto.Fields)
                {
                    var dynamicField = new MedicalVisitDynamicField
                    {
                        Id = Guid.NewGuid(),
                        PatientMedicalVisitId = dto.PatientMedicalVisitId,
                        FieldDefinitionId = fieldValue.FieldDefinitionId,
                        FieldValue = fieldValue.FieldValue,
                        NumericValue = fieldValue.NumericValue,
                        DateValue = fieldValue.DateValue,
                        BooleanValue = fieldValue.BooleanValue
                    };

                    await _commandRepository.Create(dynamicField, cts);
                    savedCount++;
                }

                await _unitOfWork.SaveChanges(cts);
                await _outputPort.Handle(savedCount);
            }, cts);
        }
        catch (OperationCanceledException)
        {
            await _outputPort.ErrorAsync(new ErrorInfo
            {
                Code = ErrorCode.Cancelled,
                Message = "Operación cancelada por el usuario."
            });
        }
        catch (Exception ex)
        {
            await _outputPort.ErrorAsync(new ErrorInfo
            {
                Code = ErrorCode.DatabaseError,
                Message = $"Error al guardar los campos dinámicos: {ex.Message}"
            });
        }
    }
}