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
    public async Task LoadVisitsAsync(Guid patientId, CancellationToken cts = default)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            // Llamamos al input port para poblar el output port
            await _inputPort.Handle(patientId, null, cts);

            Visits.Clear();

            if (_outputPort.ListMedicalVisitSummary != null)
            {
                var list = _outputPort.ListMedicalVisitSummary
                    .Select(dto => new MedicalVisitModel(dto))
                    .OrderByDescending(v => v.VisitDate);

                foreach (var visit in list)
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
}
