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
    private Guid _selectedPatientId;
    private PaginationDto _paginationDto = new(1, 10);
    private CancellationTokenSource _debounceCts;
    #endregion

    #region Properties
    private IEnumerable<PatientSummaryDto> PagedPatients = [];
    private string Filter
    {
        get => _paginationDto.Filter;
        set
        {
            if (_paginationDto.Filter != value)
            {
                _paginationDto.Filter = value;
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
    private void SelectPatient(PatientSummaryDto patient)
    {
        _selectedPatientId = patient.Id;
        _paginationDto.CurrentPage = 1; // Reset visit page when selecting a new patient
    }
    private async Task HandlePageChanged(int newPage)
    {
        _paginationDto.CurrentPage = newPage;
        await LoadPatients();
    }
    private void OnEditPatient(PatientSummaryDto patient)
    {
        _ = OnPatientUpdate.InvokeAsync(patient.Id);
    }
    private void OnDeletePatient(PatientSummaryDto patient)
    {
        _ = OnPatientDeleted.InvokeAsync((patient.Id, $"{patient.LastName}, {patient.FirstName}"));
        _ = LoadPatients();
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
        PagedPatients = VM.PatientsList;
        var total = VM.TotalRecords;
        _totalPages = (int)Math.Ceiling((double)total / _paginationDto.PageSize);
        FooterMessage = string.Format(ListPatientsMessages.TotalPatientMessageTemplate, total);
        StateHasChanged(); // Forzar la actualización del componente
    }
    #endregion
}