using MedRec.Entity.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.ViewModels.Models;

namespace MedRec.HealthInsurance.ViewModels.VM;
public class HealthInsuranceCatalogVM(
    IHealthInsuranceCatalogInputPort inputPort,
    IHealthInsuranceCatalogOutputPort outputPort)
{
    private readonly IHealthInsuranceCatalogInputPort _inputPort = inputPort;
    private readonly IHealthInsuranceCatalogOutputPort _outputPort = outputPort;

    private string _informationMessage;
    private int _totalRecords;

    public event Action OnCatalogLoaded;
    public event Action OnShowMessage;

    public IEnumerable<HealthInsuranceModel> HealthInsuranceCatalog { get; set; } = [];

    public string InformationMessage { get => _informationMessage; set => _informationMessage = value; }

    public int TotalRecords { get => _totalRecords; set => _totalRecords = value; }

    public async Task LoadHealthCompaniesAsync(PaginationDto paginationDto, CancellationToken cts = default)
    {
        await _inputPort.Handle(paginationDto, cts);


        if (_outputPort.ValidationErrors?.Any() == true)
        {

            InformationMessage = string.Join("<br />", _outputPort.ValidationErrors.Select(e => e.ErrorMessage));
            OnShowMessage?.Invoke();
        }
        else if (_outputPort.ErrorMessage is not null)
        {
            InformationMessage = _outputPort.ErrorMessage.Message;
            OnShowMessage?.Invoke();
        }
        else
        {
            HealthInsuranceCatalog = _outputPort.HealthInsuranceCatalog.Select(dto => new HealthInsuranceModel()
            {
                Id = dto.Id,
                Name = dto.Name,
                Acronym = dto.Acronym
            }).ToList();

            TotalRecords = _outputPort.TotalRecords;

            OnCatalogLoaded?.Invoke();
        }

    }
}
