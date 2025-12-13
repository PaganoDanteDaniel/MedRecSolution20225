using MedRec.CommonComponents.Views;
using MedRec.HealthInsurance.Views.Components;
using MedRec.Patients.ViewModels.Models;
using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Helpers;
using MedRec.Patients.Views.Resources;
using MedRec.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace MedRec.Patients.Views.Components;
public partial class UpdatePatientComponent : IDisposable
{
    #region === INYECCIÓN DE DEPENDENCIAS ===

    [Inject] public NavigationManager Navigation { get; set; } = default!;

    #endregion

    #region === PARÁMETROS DEL COMPONENTE ===

    [Parameter] public UpdatePatientVM VM { get; set; } = default!;
    [Parameter] public Guid PatientId { get; set; }

    #endregion

    #region === CAMPOS PRIVADOS ===

    private HealthInsuranceSelector? _healthInsuranceSelectorRef;

    // Estado de UI
    private string _healthInsuranceCardDisplay = string.Empty;
    private bool isLoading;

    // EditContext y validación
    private EditContext _editContext = default!;

    private readonly ModalNotifier _modalNotifier = new();

    // Modelo original para detectar cambios
    private UpdatePatientModel _originalModel = default!;

    // Gestión de concurrencia
    private bool _showConcurrencyModal = true;

    // Event handlers (para poder desuscribirse)
    private Action _onShowMessageHandler = default!;
    private Action _onShowWarningHandler = default!;
    private Action _onShowErrorHandler = default!;
    private Action _onShowConcurrencyErrorHandler = default!;
    private Action _onPatientUpdatedHandler = default!;

    // Recursos gestionables
    private CancellationTokenSource _cts = new();

    #endregion

    #region === PROPIEDADES DERIVADAS ===

    /// <summary>
    /// Indica si el modelo actual difiere del modelo original (para habilitar guardado, etc.).
    /// </summary>
    private bool HasChanges => !DeepEquals(_originalModel, VM.Model);

    #endregion

    #region === CICLO DE VIDA ===

    protected override async Task OnInitializedAsync()
    {
        // 1. Configurar EditContext antes de cargar datos
        _editContext = new EditContext(VM.Model);
        _modalNotifier.OnStateChanged += StateHasChanged;

        // 2. Cargar datos del paciente
        await VM.GetPatient(PatientId, _cts.Token);
        _originalModel = VM.Model.Clone();

        // 3. Suscribirse a eventos del ViewModel
        SetupEventSubscriptions();

        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(VM.Model.HealthInsuranceCard))
        {
            _healthInsuranceCardDisplay = FormatCardNumberLinq(
                VM.Model.HealthInsuranceCard.Replace(" ", "")
            );
        }

        base.OnParametersSet();
    }

    public void Dispose()
    {
        // Desuscribirse de eventos para evitar fugas de memoria
        if (VM != null)
        {
            VM.OnShowMessage -= _onShowMessageHandler;
            VM.OnShowWarning -= _onShowWarningHandler;
            VM.OnShowError -= _onShowErrorHandler;
            VM.OnShowConcurrencyError -= _onShowConcurrencyErrorHandler;
            VM.OnPatientUpdated -= _onPatientUpdatedHandler;
        }
        _modalNotifier.OnStateChanged -= StateHasChanged;
        _cts?.Dispose();
    }

    #endregion

    #region === MANEJO DE EVENTOS DEL VIEWMODEL ===

    private void SetupEventSubscriptions()
    {
        _onShowMessageHandler = () => _modalNotifier.ShowMessage("Información", ModalType.MessageInfo, VM.InformationMessage);
        _onShowWarningHandler = () => _modalNotifier.ShowMessage("Advertencia", ModalType.MessageWarning, VM.InformationMessage);
        _onShowErrorHandler = () => _modalNotifier.ShowMessage("Error", ModalType.MessageError, VM.InformationMessage);
        _onShowConcurrencyErrorHandler = () => _modalNotifier.ShowMessage("Conflicto de concurrencia", ModalType.MessageError, VM.InformationMessage);
        _onPatientUpdatedHandler = () => _modalNotifier.ShowMessageAndNavigate("Actualización exitosa...", ModalType.MessageSuccess, "/list-patients", VM.InformationMessage);


        VM.OnShowMessage += _onShowMessageHandler;
        VM.OnShowWarning += _onShowWarningHandler;
        VM.OnShowError += _onShowErrorHandler;
        VM.OnShowConcurrencyError += _onShowConcurrencyErrorHandler;
        VM.OnPatientUpdated += _onPatientUpdatedHandler;
    }

    #endregion

    #region === ACCIONES DE USUARIO ===

    private async Task UpdatePatient()
    {
        isLoading = true;
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            await VM.UpdatePatient(_cts.Token);
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ExitForm()
    {
        Navigation.NavigateTo("/patient-control", true);
    }

    #endregion

    #region === MANEJO DE SELECCIÓN DE OBRA SOCIAL ===

    private void OnHealthCompanySelected((Guid id, string name) selectedCompany)
    {
        VM.Model.HealthInsuranceCompanyId = selectedCompany.id;
        VM.Model.SelectedHealthCompanyName = selectedCompany.name;
    }

    #endregion

    #region === MANEJO DE TARJETA DE OBRA SOCIAL ===

    private void OnHealthInsuranceCardInput(ChangeEventArgs e)
    {
        var input = e.Value?.ToString() ?? string.Empty;
        var cleanValue = input.Replace(" ", "");
        VM.Model.HealthInsuranceCard = cleanValue;
        _healthInsuranceCardDisplay = FormatCardNumberLinq(cleanValue);
        OnFieldChanged(() => VM.Model.HealthInsuranceCard);
    }

    private string FormatCardNumberLinq(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        return string.Concat(
            value.Select((c, i) => i > 0 && i % 4 == 0 ? $" {c}" : c.ToString())
        );
    }

    #endregion

    #region === VALIDACIÓN Y UTILIDADES ===
    private void OnConcurrencyCancel()
    {
        _showConcurrencyModal = false;
        DismissConcurrencyPanel(); // tu lógica original
    }

    /// <summary>
    /// Notifica al EditContext que un campo específico ha cambiado (útil tras actualizaciones programáticas).
    /// </summary>
    private void OnFieldChanged<T>(Expression<Func<T>> propertyExpression)
    {
        var fieldIdentifier = FieldIdentifier.Create(propertyExpression);
        _editContext?.NotifyFieldChanged(fieldIdentifier);
    }

    private bool DeepEquals(UpdatePatientModel a, UpdatePatientModel b)
    {
        if (a == null || b == null) return a == b;

        return a.FirstName == b.FirstName &&
               a.LastName == b.LastName &&
               a.DocumentNumber == b.DocumentNumber &&
               a.DateOfBirth == b.DateOfBirth &&
               a.PhoneNumber == b.PhoneNumber &&
               a.Address == b.Address &&
               a.Email == b.Email &&
               a.BiologicalSexId == b.BiologicalSexId &&
               a.HealthInsuranceCompanyId == b.HealthInsuranceCompanyId &&
               a.HealthInsuranceCard == b.HealthInsuranceCard &&
               a.HealthInsuranceMemberNumber == b.HealthInsuranceMemberNumber &&
               a.HealthInsurancePlan == b.HealthInsurancePlan;
    }

    private string GetFriendlyLabel(string propertyName)
    {
        return propertyName switch
        {
            nameof(VM.Model.FirstName) => UpdatePatientMessages.FirstNameLabel,
            nameof(VM.Model.LastName) => UpdatePatientMessages.LastNameLabel,
            nameof(VM.Model.DocumentNumber) => UpdatePatientMessages.DocumentNumberLabel,
            nameof(VM.Model.BiologicalSexId) => UpdatePatientMessages.BiologicalSexLabel,
            nameof(VM.Model.DateOfBirth) => UpdatePatientMessages.DateOfBirthLabel,
            nameof(VM.Model.PhoneNumber) => UpdatePatientMessages.PhoneNumberLabel,
            nameof(VM.Model.Address) => UpdatePatientMessages.AddresLabel,
            nameof(VM.Model.Email) => UpdatePatientMessages.EmailLabel,
            nameof(VM.Model.HealthInsuranceMemberNumber) => UpdatePatientMessages.HealthInsuranceMemberNumberLabel,
            nameof(VM.Model.HealthInsurancePlan) => UpdatePatientMessages.HealthInsurancePlanLabel,
            _ => propertyName
        };
    }

    #endregion

    #region === MANEJO DE CONCURRENCIA ===

    private void OnConcurrencyShowChanged(bool value)
    {
        _showConcurrencyModal = value;
        if (!value)
        {
            DismissConcurrencyPanel();
        }
    }

    private void OnResolutionChanged(ConflictFieldModel conflict, string? value)
    {
        if (Enum.TryParse<ResolutionChoice>(value, out var choice))
        {
            conflict.Resolution = choice;
        }
    }

    private async Task RetryWithResolvedValues()
    {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        await VM.RetryWithResolvedValues(_cts.Token);
    }

    private void DismissConcurrencyPanel()
    {
        VM.ConcurrencyConflicts.Clear();
    }

    #endregion
}