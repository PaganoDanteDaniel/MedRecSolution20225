using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.BusinessObjects.DTOs;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;

namespace MedRec.MedicalVisit.Presenters.Implementations;

internal class GetMedicalVisitPresenter : BaseOutputPort<GetMedicalVisitDto>, IGetMedicalVisitOutputPort
{
    public Task Handle(PatientMedicalVisit medicalVisit, CancellationToken cts = default)
    {

        var visit = new GetMedicalVisitDto
        {
            Id = medicalVisit.Id,
            MedicalHistoryId = medicalVisit.MedicalHistoryId,
            VisitDate = medicalVisit.VisitDate,
            Reason = medicalVisit.Reason,
            Diagnosis = medicalVisit.Diagnosis,
            Treatment = medicalVisit.Treatment,
            SystolicPressure = medicalVisit.SystolicPressure,
            DiastolicPressure = medicalVisit.DiastolicPressure,
            PulsePerMinute = medicalVisit.PulsePerMinute,
            Temperature = medicalVisit.Temperature,
            Notes = medicalVisit.Notes,
            RowVersion = medicalVisit.RowVersion
        };
        Result = OperationResult<GetMedicalVisitDto>.Ok(visit);
        return Task.CompletedTask;
    }
}
