using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Patients.UseCases.Implementations;
/// <summary>
/// Interactor para listar pacientes.
/// </summary>
/// <param name="presenter">Puerto de salida para proveer la lista de pacientes.</param>
/// <param name="queriesRepository">Unidad de trabajo para manejar las operaciones de pacientes.</param>
internal class PatientListInteractor
    (IPatientsListOutputPort presenter,
    IPatientQueriesRepository queriesRepository) : IPatientsListInputPort
{
    public async Task Handle(PaginationDto paginationDto, CancellationToken cancellationToken = default)
    {
        if (paginationDto.PageNumber < 1 || paginationDto.PageSize < 1)
        {
            await presenter.ErrorAsync(new ErrorInfo("La página y el tamaño deben ser mayores a cero.", ErrorCode.Unknown));
            return;
        }
        var countResult = await queriesRepository.CountPatients(paginationDto.Filter, cancellationToken);

        if (!countResult.IsSuccess)
        {
            var error = countResult.Error ?? new ErrorInfo("Error al obtener el total de pacientes", ErrorCode.Unknown);
            await presenter.ErrorAsync(error);
            return;
        }

        var listResult = await queriesRepository.GetPatientsList(paginationDto, cancellationToken);

        if (!listResult.IsSuccess)
        {
            var error = listResult.Error ?? new ErrorInfo("Error al obtener la lista de pacientes", ErrorCode.Unknown);
            await presenter.ErrorAsync(error);
            return;
        }

        await presenter.Handle(listResult.Value, countResult.Value);
    }
}

