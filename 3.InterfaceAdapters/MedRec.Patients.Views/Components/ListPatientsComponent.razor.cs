using MedRec.Entity.DTOs;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Resources;
using Microsoft.AspNetCore.Components;

namespace MedRec.Patients.Views.Components;
public partial class ListPatientsComponent
{
    // VM del scope del componente (proporcionado por OwningComponentBase en el .razor)
    private PatientsListVM VM => Service;

    #region Fields
    private int _totalPages = 1;
    private string _footerMessage = "";
    private PaginationDto _paginationDto = new(1, 10);
    private CancellationTokenSource _debounceCts;
    private bool _showDeleteConfirmation;
    private PatientSummaryDto _patientToDelete;
    private TaskCompletionSource<bool> _deleteConfirmationTcs;
    private IEnumerable<PatientSummaryDto> pagedPatients = [];
    #endregion

    #region Properties
    private string Filter
    {
        get => _paginationDto.FilterOne;
        set
        {
            if (_paginationDto.FilterOne != value)
            {
                _paginationDto.FilterOne = value;
                _ = OnSearchTermChanged(value);
            }
        }
    }
    #endregion

    #region Injected Services
    [Inject] private NavigationManager Navigation { get; set; }
    #endregion

    #region Parameters

    [Parameter] public bool ShowActionsColumn { get; set; } = true;
    [Parameter] public bool WithHeight { get; set; } = true;
    [Parameter] public int MaxPageButton { get; set; }
    [Parameter]
    public string FooterMessage
    {
        get => _footerMessage;
        set
        {
            if (_footerMessage != value)
            {
                _footerMessage = value;
                FooterMessageParameterChanged.InvokeAsync(value);
            }
        }
    }
    #endregion

    #region Events Callbacks
    [Parameter] public EventCallback<(Guid Id, string name, string phone)> OnPatientSelected { get; set; }
    [Parameter] public EventCallback<(Guid, string)> OnPatientDeleted { get; set; }
    [Parameter] public EventCallback<Guid> OnPatientUpdate { get; set; }
    [Parameter] public EventCallback<string> FooterMessageParameterChanged { get; set; }
    #endregion

    #region Methods
    protected override async Task OnInitializedAsync() => await LoadPatients();

    private async Task OnSearchTermChanged(string searchTerm)
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();

        if (searchTerm.Length > 2 || searchTerm.Length == 0)
        {
            try
            {
                await Task.Delay(600, _debounceCts.Token);
                _paginationDto.CurrentPage = 1;
                await LoadPatients();
            }
            catch (TaskCanceledException) { }
        }
    }

    private void OnAddNewPatient() => Navigation.NavigateTo("/create-patient");

    private void SelectPatient(PatientSummaryDto patient) =>
        _ = OnPatientSelected.InvokeAsync((patient.Id, $"{patient.LastName}, {patient.FirstName}", patient.PhoneNumber));

    private async Task HandlePageChanged(int newPage)
    {
        _paginationDto.CurrentPage = newPage;
        await LoadPatients();
    }

    private void OnEditPatient(PatientSummaryDto patient) =>
        _ = OnPatientUpdate.InvokeAsync(patient.Id);

    // Cambiado a async Task (evitar async void)
    private async Task OnDeletePatient(PatientSummaryDto patient)
    {
        _patientToDelete = patient;
        _showDeleteConfirmation = true;
        _deleteConfirmationTcs = new TaskCompletionSource<bool>();
        StateHasChanged();

        bool confirmed = await _deleteConfirmationTcs.Task;
        if (confirmed)
        {
            await VM.DeleteAsync(patient.Id);
            await LoadPatients();
        }

        _patientToDelete = null;
        _showDeleteConfirmation = false;
        StateHasChanged();
    }

    private void OnAddMedicalVisit(PatientSummaryDto patient)
    {
        var nombreCodificado = System.Web.HttpUtility.UrlEncode($"{patient.LastName}, {patient.FirstName}");
        var fechaCodificada = patient.DateOfBirth.ToString("yyyy-MM-dd");
        var url = $"/medical-visit/list?id={patient.Id}&nombre={nombreCodificado}&fechaNac={fechaCodificada}";
        Navigation.NavigateTo(url, true);
    }

    private async Task LoadPatients()
    {
        await VM.LoadPatientsAsync(_paginationDto);
        pagedPatients = VM.PatientsList;
        var total = VM.TotalRecords;
        _totalPages = (int)Math.Ceiling((double)total / _paginationDto.PageSize);
        FooterMessage = string.Format(ListPatientsMessages.TotalPatientMessageTemplate, total);
        StateHasChanged();
    }

    private void OnAcceptDelete() => _deleteConfirmationTcs?.SetResult(true);
    private void OnCloseDeleteModal(bool value)
    {
        _showDeleteConfirmation = value;
        _deleteConfirmationTcs?.SetResult(false);
    }
    #endregion
}