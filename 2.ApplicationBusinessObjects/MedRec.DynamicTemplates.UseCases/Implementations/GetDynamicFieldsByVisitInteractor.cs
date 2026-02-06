using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;

namespace MedRec.DynamicTemplates.UseCases.Implementations;

/// <summary>
/// Interactor for GetDynamicFieldsByVisit use case
/// </summary>
internal class GetDynamicFieldsByVisitInteractor : IGetDynamicFieldsByVisitInputPort
{
    private readonly IGetDynamicFieldsByVisitOutputPort _outputPort;
    private readonly IMedicalVisitDynamicFieldQueriesRepositoryUoW _queriesRepository;

    public GetDynamicFieldsByVisitInteractor(
        IGetDynamicFieldsByVisitOutputPort outputPort,
        IMedicalVisitDynamicFieldQueriesRepositoryUoW queriesRepository)
    {
        _outputPort = outputPort;
        _queriesRepository = queriesRepository;
    }

    public async Task Handle(Guid visitId, CancellationToken cts = default)
    {
        try
        {
            cts.ThrowIfCancellationRequested();

            var fields = await _queriesRepository.GetByVisitId(visitId, cts);

            if (!fields.Any())
            {
                await _outputPort.HandleNotFound();
                return;
            }

            var dtos = fields.Select(f => new DynamicFieldValueDto
            {
                FieldDefinitionId = f.FieldDefinitionId,
                FieldValue = f.FieldValue,
                NumericValue = f.NumericValue,
                DateValue = f.DateValue,
                BooleanValue = f.BooleanValue
            }).ToList();

            await _outputPort.Handle(dtos);
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
                Message = $"Error al obtener los campos dinámicos de la visita: {ex.Message}"
            });
        }
    }
}