using MedRec.DynamicTemplates.BusinessObjects.DTOs;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

public interface ISaveDynamicFieldsInputPort
{
    Task Handle(SaveDynamicFieldsDto dto, CancellationToken cts = default);
}