namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

public interface IGetActiveSpecialtiesInputPort
{
    Task Handle(CancellationToken cts = default);
}