using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.Entity.POCOEntities;

namespace MedRec.MedicalVisit.BusinessObjects.DTOs;

public class CreateMedicalVisitDto
{
    public Guid MedicalHistoryId { get; init; }
    public Guid SpecialtyId { get; init; }
    public DateTime VisitDate { get; init; }
    public string Reason { get; init; }
    public string Diagnosis { get; init; }
    public string Treatment { get; init; }
    public string Notes { get; init; }
    public Guid DoctorId { get; init; }
    public IEnumerable<DynamicFieldValueDto> DynamicFields { get; init; } = Enumerable.Empty<DynamicFieldValueDto>();
    public byte[] RowVersion { get; init; }

    public static explicit operator PatientMedicalVisit(CreateMedicalVisitDto dto) =>
        new()
        {
            MedicalHistoryId = dto.MedicalHistoryId,
            VisitDate = dto.VisitDate,
            Reason = dto.Reason,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            RowVersion = dto.RowVersion
        };
}
