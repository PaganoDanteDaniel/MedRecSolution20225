using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.HealthInsurance.Presenters.Implementations;
internal class GetHealthInsuranceByIdPresenter : IGetHealthInsuranceByIdOutputPort
{
    private GetHealthInsuranceDto _getHealthInsurance;
    private ErrorInfo? _errorMessage;
    private IReadOnlyList<ValidationError> _validationErrors = Array.Empty<ValidationError>();
    public GetHealthInsuranceDto HealthInsurance => _getHealthInsurance;

    public IEnumerable<ValidationError> ValidationErrors => _validationErrors;

    public ErrorInfo ErrorMessage => _errorMessage;

    public Task Handle(HealthInsuranceCompany healthInsurance, CancellationToken ct = default)
    {
        _getHealthInsurance = new GetHealthInsuranceDto
        {
            Id = healthInsurance.Id,
            Name = healthInsurance.Name,
            Acronym = healthInsurance.Acronym,
            RowVersion = healthInsurance.RowVersion
        };
        return Task.CompletedTask;
    }

    public Task ErrorAsync(ErrorInfo message)
    {
        _errorMessage = message ?? new ErrorInfo("Error desconocido.");
        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        _validationErrors = (errors ?? Enumerable.Empty<ValidationError>()).ToArray();
        return Task.CompletedTask;

    }
}
