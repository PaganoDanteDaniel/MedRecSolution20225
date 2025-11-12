using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.HealthInsurance.BusinessObjects.DTOs;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.ViewModels.Models;

namespace MedRec.HealthInsurance.ViewModels.VM;
public class CreateHealthInsuranceVM(
    ICreateHealthInsuranceInputPort createInputPort,
    ICreateHealthInsuranceOutputPort createPresenter)
{

    private string _informationMessage;

    #region Events
    public event Action OnHealthInsuranceAdded;
    public event Action OnHealthInsuranceUpdated;
    public event Action OnHealthInsuranceDeleted;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;
    #endregion

    #region Properties
    public CreateHealthInsuranceModel Model { get; set; } = new();
    public string InformationMessage
    {
        get => _informationMessage;
        set
        {
            if (_informationMessage != value)
            {
                _informationMessage = value;
            }
        }
    }
    #endregion

    #region Method
    public async Task AddHealthCompany()
    {
        InformationMessage = "";
        try
        {
            var insuranceHealthCompany = (CreateHealthInsuranceDto)Model;
            await createInputPort.Handle(insuranceHealthCompany);
            if (createPresenter.ErrorMessage is not null)
            {
                HandleErrors(createPresenter.ErrorMessage);
            }
            else
            {
                Model.Name = "";
                Model.Acronym = "";
                OnHealthInsuranceAdded?.Invoke();
            }

        }
        catch (Exception ex)
        {
            InformationMessage = ex.Message;
        }
    }

    private void HandleErrors(ErrorInfo error)
    {
        InformationMessage = error.Message;

        switch (error.Code)
        {
            case ErrorCode.DuplicateKey:
                OnShowWarning?.Invoke();
                break;
            case ErrorCode.ConcurrencyError:
                OnShowConcurrencyError?.Invoke();
                break;
            case ErrorCode.DatabaseError:
                OnShowError?.Invoke();
                break;
            default:
                OnShowMessage?.Invoke();
                break;
        }
    }
    #endregion
}
