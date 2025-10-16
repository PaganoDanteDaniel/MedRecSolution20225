namespace MedRec.MedicalVisit.BusinessObjects.DTOs;
public class CreatePatientLaboratoryResultDto()
{
    public int ResultTypeId { get; init; }
    public DateTime ResultDate { get; init; }
    public string ResultValue { get; init; }
    public string ResultNotes { get; init; }
    public bool IsDeleted { get; init; }
}
