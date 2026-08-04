using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalVisit.Presenters.Implementations;

internal class MedicalVisitSummaryListPresenter : BaseOutputPort<IEnumerable<MedicalVisitSummaryDto>>, IMedicalVisitSummaryListOutputPort
{
    public Task Handle(IEnumerable<PatientMedicalVisit> listMedicalVisit)
    {
        var dto = listMedicalVisit.Select(v => new MedicalVisitSummaryDto
        {
            Id = v.Id,
            VisitDate = v.VisitDate,
            Reason = v.Reason,
            Diagnosis = v.Diagnosis,
            Treatment = v.Treatment,
            Notes = v.Notes
        }).ToList();

        Result = OperationResult<IEnumerable<MedicalVisitSummaryDto>>.Ok(dto);

        return Task.CompletedTask;
    }
}
