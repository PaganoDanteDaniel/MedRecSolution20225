using MedRec.CommonComponents.Views;
using MedRec.HealthInsurance.Views.Components;
using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace MedRec.Patients.Views.Components;
public partial class CreatePatientComponent : IDisposable
{
    #region === ENUMERACIONES ===

    private enum NotificationSource
    {
        Patient,
        HealthInsuranceAdd,
        HealthInsuranceUpdate
    }

    #endregion

    #region === INYECCIÓN DE DEPENDENCIAS ===

    [Inject] public NavigationManager Navigation { get; set; } = default!;

    #endregion

    #region === PARÁMETROS DEL COMPONENTE ===

    [Parameter] public CreatePatientVM VM { get; set; } = default!;

    #endregion

    #region === CAMPOS PRIVADOS ===

    // EditContext y estado de carga
    private EditContext _editContext = default!;
    private bool isLoading;

    private readonly ModalNotifier _modalNotifier = new();

    // Referencias a componentes
    private HealthInsuranceSelector? _healthInsuranceSelectorRef;

    // Recursos gestionables
    private CancellationTokenSource _cts = new();
    private string _healthInsuranceCardDisplay = string.Empty;

    // Event handlers (para desuscripción segura)
    private Action _onPatientAdded = default!;
    private Action _onShowMessage = default!;
    private Action _onShowWarning = default!;
    private Action _onShowError = default!;
    private Action _onShowConcurrencyError = default!;

    #endregion

    #region === CICLO DE VIDA ===

    protected override void OnInitialized()
    {
        _editContext = new EditContext(VM.Model);
        _modalNotifier.OnStateChanged += StateHasChanged;
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
        if (VM != null)
        {
            VM.OnPatientAdded -= _onPatientAdded;
            VM.OnShowMessage -= _onShowMessage;
            VM.OnShowWarning -= _onShowWarning;
            VM.OnShowError -= _onShowError;
            VM.OnShowConcurrencyError -= _onShowConcurrencyError;
        }
        _modalNotifier.OnStateChanged -= StateHasChanged;

        _cts?.Dispose();
    }

    #endregion

    #region === SUSCRIPCIÓN A EVENTOS DEL VIEWMODEL ===

    private void SetupEventSubscriptions()
    {
        _onPatientAdded = () => _modalNotifier.ShowMessageAndNavigate("Registro exitoso...", ModalType.MessageSuccess, "/patient-control", VM.InformationMessage);
        _onShowMessage = () => _modalNotifier.ShowMessage("Información", ModalType.MessageInfo, VM.InformationMessage);
        _onShowWarning = () => _modalNotifier.ShowMessage("Advertencia", ModalType.MessageWarning, VM.InformationMessage);
        _onShowError = () => _modalNotifier.ShowMessage("Error", ModalType.MessageError, VM.InformationMessage);
        _onShowConcurrencyError = () => _modalNotifier.ShowMessage("Conflicto de concurrencia", ModalType.MessageError, VM.InformationMessage);

        VM.OnPatientAdded += _onPatientAdded;
        VM.OnShowMessage += _onShowMessage;
        VM.OnShowWarning += _onShowWarning;
        VM.OnShowError += _onShowError;
        VM.OnShowConcurrencyError += _onShowConcurrencyError;
    }

    #endregion

    #region === ACCIONES DE USUARIO ===

    public async Task AddPatient()
    {
        if (!_editContext.Validate()) return;

        isLoading = true;
        StateHasChanged();
        await Task.Yield();

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            await VM.AddPatientAsync(_cts.Token);
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ExitForm(MouseEventArgs e)
    {
        Navigation.NavigateTo("/patient-control", true);
    }

    #endregion

    #region === MANEJO DE OBRA SOCIAL ===

    private void OnHealthCompanySelected((Guid Id, string Name) selectedCompany)
    {
        VM.Model.HealthInsuranceCompanyId = selectedCompany.Id;
        VM.Model.SelectedHealthCompanyName = selectedCompany.Name;
    }

    #endregion

    #region === FORMATO DE TARJETA DE OBRA SOCIAL ===

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

    #region === UTILIDADES DE VALIDACIÓN ===

    /// <summary>
    /// Notifica al EditContext que un campo específico ha cambiado,
    /// activando su validación sin interacción del usuario.
    /// Útil tras actualizaciones programáticas del modelo.
    /// </summary>
    private void OnFieldChanged<T>(Expression<Func<T>> propertyExpression)
    {
        var fieldIdentifier = FieldIdentifier.Create(propertyExpression);
        _editContext?.NotifyFieldChanged(fieldIdentifier);
    }

    #endregion
}