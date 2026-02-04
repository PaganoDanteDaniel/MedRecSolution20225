namespace MedRec.DynamicTemplates.BusinessObjects.DTOs;

public class SaveDynamicFieldsDto
{
    public SaveDynamicFieldsDto(Guid visitId, IEnumerable<DynamicFieldValueDto> fields)
    {
        PatientMedicalVisitId = visitId;
        Fields = fields;
    }

    public Guid PatientMedicalVisitId { get; init; }
    public IEnumerable<DynamicFieldValueDto> Fields { get; init; }
}