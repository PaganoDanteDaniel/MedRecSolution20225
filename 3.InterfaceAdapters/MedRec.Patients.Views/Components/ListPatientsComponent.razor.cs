using MedRec.CommonComponents.Views;
using MedRec.Entity.DTOs;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Helpers;
using MedRec.Patients.Views.Resources;
using Microsoft.AspNetCore.Components;

namespace MedRec.Patients.Views.Components;
public partial class ListPatientsComponent
{
    #region === PROPIEDAD DE VIEWMODEL ===

    /// <summary>
    /// ViewModel proporcionado por OwningComponentBase en el archivo .razor.
    /// </summary>
    private PatientsListVM VM => Service;

    private readonly ModalNotifier _modalNotifier = new();
    #endregion

    #region === INYECCIÓN DE DEPENDENCIAS ===

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    #endregion

    #region === PARÁMETROS DEL COMPONENTE ===

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
                _ = FooterMessageParameterChanged.InvokeAsync(value);
            }
        }
    }

    #endregion

    #region === EVENTOS DE SALIDA (CALLBACKS) ===

    [Parameter] public EventCallback<(Guid Id, string name, string phone)> OnPatientSelected { get; set; }
    [Parameter] public EventCallback<(Guid, string)> OnPatientDeleted { get; set; }
    [Parameter] public EventCallback<Guid> OnPatientUpdate { get; set; }
    [Parameter] public EventCallback<string> FooterMessageParameterChanged { get; set; }

    #endregion

    #region === CAMPOS PRIVADOS ===

    // Paginación y búsqueda
    private PaginationDto _paginationDto = new(1, 10);
    private int _totalPages = 1;
    private string _footerMessage = string.Empty;
    private CancellationTokenSource _debounceCts = new();

    // Estado de carga
    private bool isLoading;

    // Pacientes paginados
    private IEnumerable<PatientSummaryDto> pagedPatients = [];

    // Confirmación de eliminación
    private PatientSummaryDto _patientToDelete = default!;
    private TaskCompletionSource<bool>? _deleteConfirmationTcs;

    // Handlers de eventos (para desuscripción)
    private Action _onPatientDeleted = default!;
    private Action _onShowMessage = default!;
    private Action _onShowWarning = default!;
    private Action _onShowError = default!;
    private Action _onShowConcurrencyError = default!;

    #endregion

    #region === Callbacks para el nuevo modal ===

    public EventCallback _onOkCallback = default;
    public EventCallback _onCancelCallback = default;
    public EventCallback _onDeleteCallback = default;
    public EventCallback _onRetryCallback = default;

    #endregion
    #region === PROPIEDADES DERIVADAS ===

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

    #region === CICLO DE VIDA ===
    protected override async Task OnInitializedAsync()
    {
        _modalNotifier.OnStateChanged += StateHasChanged;
        isLoading = true;
        SetupEventSubscriptions();
        await LoadPatients();
        StateHasChanged();
    }

    public void Dispose()
    {
        if (VM != null)
        {
            VM.OnShowMessage -= _onShowMessage;
            VM.OnShowWarning -= _onShowWarning;
            VM.OnShowError -= _onShowError;
            VM.OnShowConcurrencyError -= _onShowConcurrencyError;
            VM.OnPatientDeleted -= _onPatientDeleted;
        }

        _modalNotifier.OnStateChanged -= StateHasChanged;

        _debounceCts?.Dispose();
    }
    #endregion

    #region === SUSCRIPCIÓN A EVENTOS DEL VIEWMODEL ===

    private void SetupEventSubscriptions()
    {
        _onPatientDeleted = async () =>
        {
            await LoadPatients(); // recargar lista tras eliminación
            _onOkCallback = EventCallback.Factory.Create(this, OnCloseModal);
            _modalNotifier.ShowMessage("Eliminación exitosa...", ModalType.MessageSuccess, "PACIENTE ELIMINADO EXITOSAMENTE");
        };

        _onShowMessage = () => _modalNotifier.ShowInfoModal("Información", VM.InformationMessage);
        _onShowWarning = () => _modalNotifier.ShowWarningModal("Advertencia", VM.InformationMessage);
        _onShowError = () => _modalNotifier.ShowErrorModal("Error", VM.InformationMessage);
        _onShowConcurrencyError = () => _modalNotifier.ShowErrorModal("Conflicto de concurrencia", VM.InformationMessage);

        VM.OnShowMessage += _onShowMessage;
        VM.OnShowWarning += _onShowWarning;
        VM.OnShowError += _onShowError;
        VM.OnShowConcurrencyError += _onShowConcurrencyError;
        VM.OnPatientDeleted += _onPatientDeleted;
    }

    #endregion

    #region === CARGA Y FILTRADO ===

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
            catch (TaskCanceledException)
            {
                // Ignorar cancelaciones por nuevo término de búsqueda
            }
        }
    }

    private async Task LoadPatients()
    {
        await VM.LoadPatientsAsync(_paginationDto);
        pagedPatients = VM.PatientsList;
        var total = VM.TotalRecords;
        _totalPages = (int)Math.Ceiling((double)total / _paginationDto.PageSize);
        FooterMessage = string.Format(ListPatientsMessages.TotalPatientMessageTemplate, total);
        isLoading = false;
        StateHasChanged();
    }

    private async Task HandlePageChanged(int newPage)
    {
        _paginationDto.CurrentPage = newPage;
        await LoadPatients();
    }

    #endregion

    #region === ACCIONES DE USUARIO ===

    private void OnAddNewPatient() => Navigation.NavigateTo("/create-patient");
    private void SelectPatient(PatientSummaryDto patient) =>
        _ = OnPatientSelected.InvokeAsync((patient.Id, $"{patient.LastName}, {patient.FirstName}", patient.PhoneNumber));

    private void OnEditPatient(PatientSummaryDto patient) =>
        _ = OnPatientUpdate.InvokeAsync(patient.Id);

    private void OnAddMedicalVisit(PatientSummaryDto patient)
    {
        var nombreCodificado = System.Web.HttpUtility.UrlEncode($"{patient.LastName}, {patient.FirstName}");
        var fechaCodificada = patient.DateOfBirth.ToString("yyyy-MM-dd");
        var url = $"/medical-visit/list?id={patient.Id}&nombre={nombreCodificado}&fechaNac={fechaCodificada}";
        Navigation.NavigateTo(url, true);
    }
    private void OnDeletePatient(PatientSummaryDto patient)
    {
        _patientToDelete = patient;

        // Mostrar modal de confirmación
        var message = string.Format(
            ListPatientsMessages.ModalMessageTextTemplate,
            string.Format(ListPatientsMessages.ModalMessageText, $"{(_patientToDelete?.LastName ?? "")}, {(_patientToDelete?.FirstName ?? "")}")
        );

        _onDeleteCallback = EventCallback.Factory.Create(this, OnAcceptDelete);
        _onCancelCallback = EventCallback.Factory.Create(this, OnCloseModal);

        _modalNotifier.ShowDeleteModal(
            ListPatientsMessages.ModalTitleText,
            message
        );
    }
    private async Task OnAcceptDelete()
    {
        await VM.DeleteAsync(_patientToDelete.Id);
        _patientToDelete = null;
    }
    private void OnCloseModal()
    {
        _modalNotifier.IsVisible = false;
    }

    #endregion

}