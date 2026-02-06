using MedRec.BusinessObjects.Interfaces;

namespace MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

/// <summary>
/// Output port for SaveDynamicFields use case.
/// Inherit from IBaseOutputPort to expose ErrorAsync / ValidationErrorsAsync.
/// </summary>
public interface ISaveDynamicFieldsOutputPort : IBaseOutputPort
{
    Task Handle(int savedCount);
    Task HandleValidationErrors(Dictionary<string, List<string>> errors);
}