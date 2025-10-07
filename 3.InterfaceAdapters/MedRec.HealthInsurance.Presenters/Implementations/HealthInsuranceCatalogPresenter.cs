using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.Validator.ValueObjects;

namespace MedRec.HealthInsurance.Presenters.Implementations;
internal class HealthInsuranceCatalogPresenter : IHealthInsuranceCatalogOutputPort
{
    public int TotalRecords { get; private set; }

    public List<HealthInsuranceCatalogDto> HealthInsuranceCatalog { get; private set; } = [];

    public IEnumerable<ValidationError> ValidationErrors { get; private set; } = [];

    public ErrorInfo ErrorMessage { get; private set; }

    public Task ErrorAsync(ErrorInfo message)
    {
        ErrorMessage = message;
        return Task.CompletedTask;
    }

    public Task Handle(IEnumerable<HealthInsuranceCompany> healthInsuranceCatalog, int totalRecords, CancellationToken c)
    {
        ErrorAsync(null);

        HealthInsuranceCatalog = healthInsuranceCatalog.Select(x => new HealthInsuranceCatalogDto(
            id: x.Id,
            name: x.Name,
            acronym: x.Acronym)).ToList();
        TotalRecords = totalRecords;

        return Task.CompletedTask;
    }

    public Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        ValidationErrors = errors.ToList();
        return Task.CompletedTask;
    }
}
