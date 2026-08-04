using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.ViewModels.Models;
using System.Collections.ObjectModel;

namespace MedRec.MedicalVisit.ViewModels.VM;

public class MedicalVisitVM
{
    private readonly IMedicalVisitSummaryListInputPort _inputPort;
    private readonly IMedicalVisitSummaryListOutputPort _outputPort;
    public ObservableCollection<MedicalVisitModel> Visits { get; } = new();

    public bool IsLoading { get; private set; }
    public ErrorInfo? LastError { get; private set; }
    public string InformationMessage { get; private set; } = string.Empty;

    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;
    public MedicalVisitVM(
        IMedicalVisitSummaryListInputPort inputPort,
        IMedicalVisitSummaryListOutputPort outputPort)
    {
        _inputPort = inputPort;
        _outputPort = outputPort;
    }

    /// <summary>
    /// Carga las visitas de un paciente y actualiza la lista de UI.
    /// </summary>
    public async Task LoadVisitsAsync(Guid patientId, PaginationDto pagination, CancellationToken cts = default)
    {
        try
        {
            IsLoading = true;
            LastError = null;
            InformationMessage = string.Empty;

            await _inputPort.Handle(patientId, pagination, cts);

            var result = _outputPort.Result;
            if (result is null)
            {
                var error = new ErrorInfo("No se recibió respuesta del presentador.", ErrorCode.Unknown);
                LastError = error;
                HandleErrors(error);
                return;
            }

            if (!result.Success)
            {
                if (result.Error is not null)
                {
                    LastError = result.Error;
                    HandleErrors(result.Error);
                }
                else if (result.HasValidationErrors)
                {
                    InformationMessage = string.Join(
                        Environment.NewLine,
                        result.ValidationErrors.Select(e => e.ErrorMessage));
                    OnShowError?.Invoke();
                }
                else
                {
                    InformationMessage = "No fue posible cargar las visitas médicas.";
                    OnShowError?.Invoke();
                }

                return;
            }

            Visits.Clear();
            if (result.Value is not null)
            {
                foreach (var visit in result.Value
                    .Select(dto => new MedicalVisitModel(dto))
                    .OrderByDescending(v => v.VisitDate))
                {
                    Visits.Add(visit);
                }
            }

            if (result.MessageAction != UserMessageAction.None)
            {
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
            }
        }
        catch (Exception ex)
        {
            var error = new ErrorInfo(ex.Message, ErrorCode.Unknown);
            LastError = error;
            HandleErrors(error);
        }
        finally
        {
            IsLoading = false;
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
}
