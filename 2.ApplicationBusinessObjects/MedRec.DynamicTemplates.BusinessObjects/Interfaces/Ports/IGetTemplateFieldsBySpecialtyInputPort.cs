namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

public interface IGetTemplateFieldsBySpecialtyInputPort
{
    Task Handle(Guid specialtyId, CancellationToken cts = default);
}