namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

public interface IGetDynamicFieldsByVisitInputPort
{
    Task Handle(Guid visitId, CancellationToken cts = default);
}