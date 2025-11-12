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
    public string InformationMessage { get; set; }

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

            // Llamamos al input port para poblar el output port
            await _inputPort.Handle(patientId, pagination, cts);

            Visits.Clear();

            if (_outputPort.ListMedicalVisitSummary != null)
            {
                var list = _outputPort.ListMedicalVisitSummary
                    .Select(dto => new MedicalVisitModel(dto))
                    .OrderByDescending(v => v.VisitDate);

                foreach (var visit in list.ToList())
                    Visits.Add(visit);
            }
        }
        catch (Exception ex)
        {
            LastError = new ErrorInfo(ex.Message, ErrorCode.Unknown);
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
