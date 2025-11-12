using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.ViewModels.Models;

namespace MedRec.HealthInsurance.ViewModels.VM;
public class UpdateHealthInsuranceVM(
    IUpdateHealthInsuranceInputPort inputPort,
    IUpdateHealthInsuranceOutputPort outputPort,
    IGetHealthInsuranceByIdInputPort getInputPort,
    IGetHealthInsuranceByIdOutputPort getOutputPort)
{
    public UpdateHealthInsuranceModel Model { get; set; } = new();
    public string InformationMessage { get; set; }

    public event Action OnUpdateSuccess;

    public async Task GetHealthInsuranceAsync(Guid id, CancellationToken ct)
    {

        await getInputPort.Handle(id, ct);

        if (getOutputPort.ValidationErrors?.Any() == true)
        {

            InformationMessage = string.Join("<br />", getOutputPort.ValidationErrors.Select(e => e.ErrorMessage));
            //OnShowMessage?.Invoke();
        }
        else if (getOutputPort.ErrorMessage is not null)
        {
            InformationMessage = getOutputPort.ErrorMessage.Message;
            //OnShowMessage?.Invoke();
        }
        else
        {
            var response = getOutputPort.HealthInsurance;
            Model.Id = response.Id;
            Model.Name = response.Name;
            Model.Acronym = response.Acronym;
            Model.RowVersion = response.RowVersion;
            //OnCatalogLoaded?.Invoke();
        }




    }
    public async Task UpdateHealthCompany(CancellationToken ct)
    {
        InformationMessage = "";
        try
        {
            var insuranceHealthCompany = (UpdateHealthInsuranceDto)Model;
            await inputPort.Handle(insuranceHealthCompany);
            Model.Name = "";
            Model.Acronym = "";
            OnUpdateSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            InformationMessage = ex.Message;
        }
    }
}
