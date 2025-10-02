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
    #endregion

    #region Properties
    private IEnumerable<PatientSummaryDto> PagedPatients = [];
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
    private async Task OnSearchTermChanged(ChangeEventArgs e)
    {
        _paginationDto.Filter = e.Value?.ToString() ?? string.Empty;
        _paginationDto.CurrentPage = 1; // Reiniciar a la primera página cuando se busca
        await LoadPatients();   // Llama al método que carga los pacientes filtrados
    }
    private void SelectPatient(Guid patientId)
    {
        _selectedPatientId = patientId;
        _paginationDto.CurrentPage = 1; // Reset visit page when selecting a new patient
    }
    private async Task HandlePageChanged(int newPage)
    {
        _paginationDto.CurrentPage = newPage;
        await LoadPatients();
    }
    private async Task OnEditPatient(Guid patientId)
    {
        await OnPatientUpdate.InvokeAsync(patientId);
    }
    private async Task OnDeletePatient(Guid patientId, string name)
    {
        await OnPatientDeleted.InvokeAsync((patientId, name));
        await LoadPatients();
    }
    private void OnAddHealthIssue(Guid patientId, string name)
    {
        Navigation.NavigateTo($"/medical-information/create/{patientId}/{name}", true);
    }
    private void OnAddMedicalVisit(Guid patientId, string name)
    {
        Navigation.NavigateTo($"/medical-visit/create/{patientId}/{name}", true);
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