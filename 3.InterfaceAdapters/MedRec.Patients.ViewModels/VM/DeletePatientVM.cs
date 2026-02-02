using MedRec.Entity.Enums;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.Patients.ViewModels.VM;
public class DeletePatientVM(
    IDeletePatientInputPort deleteInputPort,
    IDeletePatientOutputPort deleteOutputPort)
{
    private IDeletePatientInputPort _deleteInputPort = deleteInputPort;
    private IDeletePatientOutputPort _deletePresenter = deleteOutputPort;

    public event Action OnPatientDeleted;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;

    private string _informationMessage;
    public string InformationMessage { get => _informationMessage; set => _informationMessage = value; }
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
                InformationMessage = "Paciente eliminado exitosamente";
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
