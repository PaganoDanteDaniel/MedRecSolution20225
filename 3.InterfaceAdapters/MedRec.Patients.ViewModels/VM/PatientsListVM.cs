using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.Patients.ViewModels.VM;
public class PatientsListVM(
    IPatientsListInputPort listInputPort,
    IPatientsListOutputPort listOutputPort,
    IDeletePatientInputPort deleteInputPort,
    IDeletePatientOutputPort deleteOutputPort)
{
    private IPatientsListInputPort _listInputPort = listInputPort;
    private IPatientsListOutputPort _listOutputPort = listOutputPort;
    private IDeletePatientInputPort _deleteInputPort = deleteInputPort;
    private IDeletePatientOutputPort _deletePresenter = deleteOutputPort;

    private string _informationMessage;
    private int _totalRecords;


    public event Action OnPatientsLoaded;
    public event Action OnPatientDeleted;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;



    public IEnumerable<PatientSummaryDto> PatientsList { get; set; } = [];

    public string InformationMessage { get => _informationMessage; set => _informationMessage = value; }
    public int TotalRecords { get => _totalRecords; set => _totalRecords = value; }

    public async Task LoadPatientsAsync(PaginationDto paginationDto, CancellationToken cts = default)
    {

        await _listInputPort.Handle(paginationDto, cts);
        var result = _listOutputPort.Result;

        if (!result.Success)
        {
            InformationMessage = result.Error?.Message ?? "Error desconocido.";

            switch (result.MessageAction)
            {
                case UserMessageAction.ShowWarning:
                    OnShowWarning?.Invoke();
                    break;
                case UserMessageAction.ShowConcurrencyMessage:
                    OnShowConcurrencyError?.Invoke();
                    break;
                case UserMessageAction.ShowError:
                    OnShowError?.Invoke();
                    break;
                case UserMessageAction.ShowInfoMessage:
                    OnShowMessage?.Invoke();
                    break;
            }
            return;
        }
        else
        {
            PatientsList = result.Value;
            // Otra forma de escribir
            // patients.Select(p => (PatientSummaryModel)patients).ToList();
            // es
            // [.. patients.Select(p => (PatientSummaryModel)patients)];

            TotalRecords = _listOutputPort.TotalRecords.Value;

            OnPatientsLoaded?.Invoke();
        }
    }

    public async Task DeleteAsync(Guid patientId, CancellationToken cts = default)
    {
        InformationMessage = "";
        await _deleteInputPort.Handle(patientId, cts);
        var result = _deletePresenter.Result;
        try
        {
            if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "Error desconocido.";

                switch (result.MessageAction)
                {
                    case UserMessageAction.ShowWarning:
                        OnShowWarning?.Invoke();
                        break;
                    case UserMessageAction.ShowConcurrencyMessage:
                        OnShowConcurrencyError?.Invoke();
                        break;
                    case UserMessageAction.ShowError:
                        OnShowError?.Invoke();
                        break;
                    case UserMessageAction.ShowInfoMessage:
                        OnShowMessage?.Invoke();
                        break;
                }
                return;
            }
            else
            {
                InformationMessage = "";
                OnPatientDeleted?.Invoke();
            }
        }
        catch (Exception)
        {
            InformationMessage = "Error inesperado al eliminar el paciente.";
            OnShowError?.Invoke();
        }

    }
}
