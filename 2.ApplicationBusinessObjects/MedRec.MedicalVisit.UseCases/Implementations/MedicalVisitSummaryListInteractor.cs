using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;

namespace MedRec.MedicalVisit.UseCases.Implementations;
internal class MedicalVisitSummaryListInteractor
    (IMedicalVisitSummaryListOutputPort _presenter,
    IMedicalVisitQueriesRepository _queriesRepository) : IMedicalVisitSummaryListInputPort
{
    public async Task Handle(Guid patientId, PaginationDto paginationDto = default, CancellationToken cts = default)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("El ID del paciente no puede estar vacío.", nameof(patientId));

        cts.ThrowIfCancellationRequested();

        var result = await _queriesRepository.GetMedicalVisits(patientId, paginationDto, cts);

        if (!result.IsSuccess)
        {
            var error = result.Error ?? new ErrorInfo("Error al obtener la lista de visitas médicas", ErrorCode.Unknown);
            await _presenter.ErrorAsync(error);
            return;
        }

        await _presenter.Handle([.. result.Value]); // => idem que result.Value.ToList();
    }
}
