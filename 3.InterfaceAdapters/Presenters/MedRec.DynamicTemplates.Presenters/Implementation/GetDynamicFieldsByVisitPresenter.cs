using MedRec.DynamicTemplates.BusinessObjects.DTOs;
using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;

namespace MedRec.DynamicTemplates.Presenters.Implementation;

/// <summary>
/// Presenter for GetDynamicFieldsByVisit use case
/// </summary>
public class GetDynamicFieldsByVisitPresenter : IGetDynamicFieldsByVisitOutputPort
{
    public IEnumerable<DynamicFieldValueDto>? Fields { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsSuccess { get; private set; }
    public bool NotFound { get; private set; }

    /// <inheritdoc/>
    public void Handle(IEnumerable<DynamicFieldValueDto> fields)
    {
        Fields = fields;
        IsSuccess = true;
        NotFound = false;
        ErrorMessage = null;
    }

    /// <inheritdoc/>
    public void HandleError(string errorMessage)
    {
        ErrorMessage = errorMessage;
        IsSuccess = false;
        NotFound = false;
        Fields = null;
    }

    /// <inheritdoc/>
    public void HandleNotFound()
    {
        NotFound = true;
        IsSuccess = false;
        ErrorMessage = "No se encontraron campos dinámicos para la visita médica especificada.";
        Fields = null;
    }
}