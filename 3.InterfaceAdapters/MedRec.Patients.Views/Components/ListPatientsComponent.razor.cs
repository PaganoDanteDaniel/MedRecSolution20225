using MedRec.Entity.DTOs;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Resources;
using Microsoft.AspNetCore.Components;

namespace MedRec.Patients.Views.Components;
public partial class ListPatientsComponent
{
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
                // Llamar al método de debounce cada vez que el valor cambia
                _ = OnSearchTermChanged(value);
            }
        }
    }
    #endregion


    #region Injected Services
    [Inject] private NavigationManager Navigation { get; set; }
    #endregion

    #region Paremeters
    [Parameter] public PatientsListVM VM { get; set; }
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
    [Parameter] public EventCallback<(Guid, string)> OnPatientDeleted { get; set; }
    [Parameter] public EventCallback<Guid> OnPatientUpdate { get; set; }
    [Parameter] public EventCallback<string> FooterMessageParameterChanged { get; set; }
    #endregion

    #region Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadPatients();
    }
    private async Task OnSearchTermChanged(string searchTerm)
    {
        // Cancelar la búsqueda anterior si existe
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();

        // Solo buscar si hay más de 2 caracteres o está vacío
        if (searchTerm.Length > 2 || searchTerm.Length == 0)
        {
            try
            {
                // Esperar 600ms antes de buscar
                await Task.Delay(600, _debounceCts.Token);
                _paginationDto.CurrentPage = 1;
                await LoadPatients();
            }
            catch (TaskCanceledException)
            {
                // Ignorar si se cancela por nueva entrada
            }
        }
    }
    private void OnAddNewPatient()
    {
        Navigation.NavigateTo("/create-patient");
    }
    //private void SelectPatient(PatientSummaryDto patient)
    //{
    //    _selectedPatientId = patient.Id;
    //    _paginationDto.CurrentPage = 1; // Reset visit page when selecting a new patient
    //}
    private async Task HandlePageChanged(int newPage)
    {
        _paginationDto.CurrentPage = newPage;
        await LoadPatients();
    }
    private void OnEditPatient(PatientSummaryDto patient)
    {
        _ = OnPatientUpdate.InvokeAsync(patient.Id);
    }
    private async void OnDeletePatient(PatientSummaryDto patient)
    {
        _patientToDelete = patient;
        _showDeleteConfirmation = true;
        _deleteConfirmationTcs = new TaskCompletionSource<bool>();
        StateHasChanged();

        bool confirmed = await _deleteConfirmationTcs.Task;
        if (confirmed)
        {
            await VM.DeleteAsync(patient.Id);
            // Aquí puedes invocar el callback al padre (opcional, si el padre necesita saberlo)
            //await OnPatientDeleted.InvokeAsync((patient.Id, $"{patient.LastName}, {patient.FirstName}"));

            // Y recargar directamente desde aquí
            await LoadPatients(); // Esto ya tiene acceso a _paginationDto
        }

        _patientToDelete = null;
        _showDeleteConfirmation = false;
        StateHasChanged();
    }
    private void OnAddHealthIssue(PatientSummaryDto patient)
    {
        Navigation.NavigateTo($"/medical-information/create/{patient.Id}/{patient.LastName}, {patient.FirstName}", true);
    }
    private void OnAddMedicalVisit(PatientSummaryDto patient)
    {
        Navigation.NavigateTo($"/medical-visit/list/{patient.Id}/{patient.LastName}, {patient.FirstName}", true);
    }
    private async Task LoadPatients()
    {
        await VM.LoadPatientsAsync(_paginationDto);
        pagedPatients = VM.PatientsList;
        var total = VM.TotalRecords;
        _totalPages = (int)Math.Ceiling((double)total / _paginationDto.PageSize);
        FooterMessage = string.Format(ListPatientsMessages.TotalPatientMessageTemplate, total);
        StateHasChanged(); // Forzar la actualización del componente
    }
    private void OnAcceptDelete()
    {
        _deleteConfirmationTcs?.SetResult(true);
    }

    private void OnCloseDeleteModal(bool value)
    {
        _showDeleteConfirmation = value;
        _deleteConfirmationTcs?.SetResult(false);
    }
    #endregion
}