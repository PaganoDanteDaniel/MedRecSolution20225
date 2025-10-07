using MedRec.Entity.DTOs;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;

namespace MedRec.Patients.ViewModels.VM;
public class PatientsListVM(
    IPatientsListInputPort listInputPort,
    IPatientsListOutputPort listOutputPort)//,
                                           //IDeletePatientInputPort deleteInputPort,
                                           //IDeletePatientOutputPort deleteOutputPort)
{
    private IPatientsListInputPort _listInputPort = listInputPort;
    private IPatientsListOutputPort _listOutputPort = listOutputPort;
    private IDeletePatientInputPort _deleteInputPort;// = deleteInputPort;
    private IDeletePatientOutputPort _deleteOutputPort;// = deleteOutputPort;

    private string _informationMessage;
    private int _totalRecords;

    public event Action OnPatientsLoaded;
    public event Action OnPatientDeleted;
    public event Action OnShowMessage;
    public IEnumerable<PatientSummaryDto> PatientsList { get; set; } = [];

    public string InformationMessage { get => _informationMessage; set => _informationMessage = value; }
    public int TotalRecords { get => _totalRecords; set => _totalRecords = value; }

    public async Task LoadPatientsAsync(PaginationDto paginationDto, CancellationToken cts = default)
    {

        await _listInputPort.Handle(paginationDto, cts);

        if (_listOutputPort.ErrorMessage is not null)
        {
            InformationMessage = _listOutputPort.ErrorMessage.Message;
            OnShowMessage?.Invoke();
        }
        else
        {
            PatientsList = _listOutputPort.Patients;
            // Otra forma de escribir
            // patients.Select(p => (PatientSummaryModel)patients).ToList();
            // es
            // [.. patients.Select(p => (PatientSummaryModel)patients)];

            TotalRecords = _listOutputPort.TotalRecords;

            OnPatientsLoaded?.Invoke();
        }
    }

    public async Task DeleteAsync(Guid patientId, CancellationToken cts = default)
    {
        await _deleteInputPort.Handle(patientId, cts);

        if (_deleteOutputPort.ValidationErrors?.Any() == true)
        {

            InformationMessage = string.Join("<br />", [.. _deleteOutputPort.ValidationErrors.Select(e => e.ErrorMessage)]);
            OnShowMessage?.Invoke();
        }
        else if (_deleteOutputPort.ErrorMessage is not null)
        {
            InformationMessage = _deleteOutputPort.ErrorMessage.Message;
            OnShowMessage?.Invoke();
        }
        else
        {
            if (_deleteOutputPort.IsDeleted)
                OnPatientDeleted?.Invoke();
        }
    }
}
