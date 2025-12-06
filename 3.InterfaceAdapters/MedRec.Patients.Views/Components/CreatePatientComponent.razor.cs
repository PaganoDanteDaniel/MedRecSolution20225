using MedRec.CommonComponents.Views;
using MedRec.HealthInsurance.Views.Components;
using MedRec.Patients.ViewModels.VM;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace MedRec.Patients.Views.Components;
public partial class CreatePatientComponent : IDisposable
{
    private enum NotificationSource
    {
        Patient,
        HealthInsuranceAdd,
        HealthInsuranceUpdate
    }

    #region Fields
    private EditContext _editContext;

    private Guid? _healthInsuranceToUpdateId;
    private bool _showHealthCompanyList;
    private bool _showAddHealthInsurance;
    private bool _showUpdateHealthInsurance;
    private AddHealthInsuranceComponent? _addHealthInsuranceRef;
    private UpdateHealthInsuranceComponent? _updateHealthInsuranceRef;
    private NotificationSource _currentNotificationSource;

    private bool _showPatientNotification;
    private string _patientNotificationTitle = "Mensaje del sistema";
    private ModalType _patientNotificationType = ModalType.MessageInfo;
    private bool _navigateAfterPatientNotification = false;
    private string _navigationUrlAfterNotification = "/";

    private CancellationTokenSource _cts;
    private string _healthInsuranceCardDisplay = string.Empty;
    #endregion

    #region Inject Services
    [Inject] public NavigationManager Navigation { get; set; }
    #endregion

    #region Parameters
    [Parameter] public CreatePatientVM VM { get; set; }
    #endregion

    #region Actions
    private Action _onPatientAdded;
    private Action _onShowMessage;
    private Action _onShowWarning;
    private Action _onShowError;
    private Action _onShowConcurrencyError;
    #endregion
    /// <summary>
    /// Notifica al EditContext que un campo específico del formulario ha cambiado,
    /// activando así su validación sin requerir interacción directa del usuario.
    /// Útil cuando se modifican propiedades del modelo programáticamente.
    /// </summary>
    /// <typeparam name="T">El tipo de la propiedad que ha cambiado.</typeparam>
    /// <param name="propertyExpression">Expresión lambda que identifica la propiedad modificada (ej: () => VM.Model.FirstName).</param>
    private void OnFieldChanged<T>(Expression<Func<T>> propertyExpression)
    {
        var fieldIdentifier = FieldIdentifier.Create(propertyExpression);
        _editContext?.NotifyFieldChanged(fieldIdentifier);
    }
    protected override void OnInitialized()
    {
        _editContext = new EditContext(VM.Model);

        // 👇 Usa los NUEVOS métodos
        _onPatientAdded = () => ShowPatientNotificationAndNavigate("Registro exitoso...", ModalType.MessageSuccess, "/");
        _onShowMessage = () => ShowPatientNotification("Información", ModalType.MessageInfo);
        _onShowWarning = () => ShowPatientNotification("Advertencia", ModalType.MessageWarning);
        _onShowError = () => ShowPatientNotification("Error", ModalType.MessageError);
        _onShowConcurrencyError = () => ShowPatientNotification("Conflicto de concurrencia", ModalType.MessageError);

        VM.OnShowMessage += _onShowMessage;
        VM.OnShowWarning += _onShowWarning;
        VM.OnShowError += _onShowError;
        VM.OnShowConcurrencyError += _onShowConcurrencyError;
        VM.OnPatientAdded += _onPatientAdded;

        StateHasChanged();
    }
    private void ShowPatientNotificationAndNavigate(string title, ModalType type, string navigationUrl)
    {
        _patientNotificationTitle = title;
        _patientNotificationType = type;
        _navigateAfterPatientNotification = true;
        _navigationUrlAfterNotification = navigationUrl;
        _currentNotificationSource = NotificationSource.Patient;
        _showPatientNotification = true;
        InvokeAsync(StateHasChanged);
    }
    private void ShowPatientNotification(string title, ModalType type)
    {
        _patientNotificationTitle = title;
        _patientNotificationType = type;
        _navigateAfterPatientNotification = false;
        _currentNotificationSource = NotificationSource.Patient;
        _showPatientNotification = true;
        InvokeAsync(StateHasChanged);
    }
    private void OnPatientNotificationClosed()
    {
        _showPatientNotification = false;

        // Solo navegar si es notificación de paciente
        if (_navigateAfterPatientNotification && _currentNotificationSource == NotificationSource.Patient)
        {
            _navigateAfterPatientNotification = false;
            Navigation.NavigateTo(_navigationUrlAfterNotification, true);
        }

        // NO cerrar modales de obra social aquí
    }
    public async Task AddPatient()
    {
        if (_editContext.Validate() == true)
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            await VM.AddPatientAsync(_cts.Token);
        }
    }

    // === RenderFragments para los cuerpos de los modales ===
    private RenderFragment HealthInsuranceModalBody => builder =>
    {
        builder.OpenComponent<HealthInsuranceComponent>(0);
        builder.AddAttribute(1, nameof(HealthInsuranceComponent.MaxPageButton), 3);
        builder.AddAttribute(2, nameof(HealthInsuranceComponent.OnHealthCompanySelected),
            EventCallback.Factory.Create<(Guid, string)>(this, OnHealthCompanySelected));
        builder.AddAttribute(3, nameof(HealthInsuranceComponent.OnRequestAddHealthInsurance),
            EventCallback.Factory.Create(this, () =>
            {
                _showHealthCompanyList = false; // cerrar Modal B
                _showUpdateHealthInsurance = false; // cerrar Modal D
                _showAddHealthInsurance = true;  // abrir Modal C
            }));
        builder.AddAttribute(4, nameof(HealthInsuranceComponent.OnRequestUpdateHealthInsurance),
        EventCallback.Factory.Create<Guid>(this, (Guid id) =>
        {
            _healthInsuranceToUpdateId = id; // 👈 Guardamos el ID
            _showHealthCompanyList = false;
            _showAddHealthInsurance = false;
            _showUpdateHealthInsurance = true;
        }));
        builder.CloseComponent();
    };

    private RenderFragment AddHealthInsuranceModalBody => builder =>
    {
        builder.OpenComponent<AddHealthInsuranceComponent>(0);
        builder.AddAttribute(1, nameof(AddHealthInsuranceComponent.OnSuccess),
            EventCallback.Factory.Create(this, OnAddHealthInsuranceSuccess));
        builder.AddAttribute(2, nameof(AddHealthInsuranceComponent.OnError),
            EventCallback.Factory.Create<string>(this, OnAddHealthInsuranceError));
        builder.AddAttribute(3, nameof(AddHealthInsuranceComponent.OnCancel),
            EventCallback.Factory.Create(this, OnAddHealthInsuranceCancel));
        builder.AddComponentReferenceCapture(4, inst => _addHealthInsuranceRef = (AddHealthInsuranceComponent?)inst);
        builder.CloseComponent();
    };

    private RenderFragment UpdateHealthInsuranceModalBody => builder =>
    {
        if (_healthInsuranceToUpdateId.HasValue)
        {
            builder.OpenComponent<UpdateHealthInsuranceComponent>(0);
            builder.AddAttribute(1, nameof(UpdateHealthInsuranceComponent.HealthInsuranceId), _healthInsuranceToUpdateId.Value);
            builder.AddAttribute(2, nameof(UpdateHealthInsuranceComponent.OnSuccess),
                EventCallback.Factory.Create(this, OnUpdateHealthInsuranceSuccess));
            builder.AddAttribute(3, nameof(UpdateHealthInsuranceComponent.OnError),
                EventCallback.Factory.Create<string>(this, OnUpdateHealthInsuranceError));
            builder.AddAttribute(4, nameof(UpdateHealthInsuranceComponent.OnCancel),
                EventCallback.Factory.Create(this, OnUpdateHealthInsuranceCancel));
            builder.AddComponentReferenceCapture(5, inst => _updateHealthInsuranceRef = (UpdateHealthInsuranceComponent?)inst);
            builder.CloseComponent();
        }
        else
        {
            // Opcional: mensaje de error o cierre automático
            builder.AddContent(0, "No se especificó una obra social para editar.");
        }
    };

    private async Task OnAddHealthInsuranceSuccess()
    {
        await ShowNotification("Éxito", "Obra social creada correctamente.", ModalType.MessageSuccess, NotificationSource.HealthInsuranceAdd);
        _showAddHealthInsurance = false;
        await Task.Delay(100);
        _showHealthCompanyList = true; // volver a lista actualizada
    }
    private async Task OnAddHealthInsuranceCancel()
    {
        _showAddHealthInsurance = false;
        await Task.Delay(100);
        _showHealthCompanyList = true;
    }
    private async Task OnAddHealthInsuranceError(string message)
    {
        _showAddHealthInsurance = false;
        await Task.Delay(100);
        _showHealthCompanyList = true;
    }

    private async Task OnUpdateHealthInsuranceSuccess()
    {
        _healthInsuranceToUpdateId = null;
        await ShowNotification("Éxito", "Obra Social actualizada correctamente.", ModalType.MessageSuccess, NotificationSource.HealthInsuranceUpdate);
        _showUpdateHealthInsurance = false;
        await Task.Delay(100);
        _showHealthCompanyList = true;
    }

    private async Task OnUpdateHealthInsuranceCancel()
    {
        _healthInsuranceToUpdateId = null;
        _showUpdateHealthInsurance = false;
        await Task.Delay(100);
        _showHealthCompanyList = true;
    }
    private async Task OnUpdateHealthInsuranceError(string message)
    {
        _healthInsuranceToUpdateId = null;
        _showUpdateHealthInsurance = false;
        await Task.Delay(100);
        _showHealthCompanyList = true;
    }

    // Método auxiliar para notificaciones de OS (reutiliza el sistema de paciente)
    private async Task ShowNotification(string title, string message, ModalType type, NotificationSource source)
    {
        _patientNotificationTitle = title;
        _patientNotificationType = type;
        VM.InformationMessage = message;
        _currentNotificationSource = source;
        _showPatientNotification = true;

        await InvokeAsync(StateHasChanged);
    }

    private void OpenHealthCompanyList()
    {
        _showHealthCompanyList = true;
    }
    private async Task OnHealthCompanySelected((Guid id, string nameselectedCompany) selectedCompany)
    {
        VM.Model.HealthInsuranceCompanyId = selectedCompany.id;
        VM.Model.SelectedHealthCompanyName = selectedCompany.nameselectedCompany;
        _showHealthCompanyList = false; // Cerrar el modal
        await Task.CompletedTask;
    }
    private void ExitForm(MouseEventArgs e)
    {
        Navigation.NavigateTo("/", true);
    }

    private void OnHealthInsuranceCardInput(ChangeEventArgs e)
    {
        var input = e.Value?.ToString() ?? string.Empty;

        // Remover espacios para obtener solo caracteres válidos
        var cleanValue = input.Replace(" ", "");

        // Guardar en el modelo SIN espacios (para la DB)
        VM.Model.HealthInsuranceCard = cleanValue;

        // Formatear para visualización con espacios cada 4 caracteres
        _healthInsuranceCardDisplay = FormatCardNumberLinq(cleanValue);

        // Notificar cambio para validación
        OnFieldChanged(() => VM.Model.HealthInsuranceCard);
    }

    private string FormatCardNumberLinq(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Concat(
            value.Select((c, i) => i > 0 && i % 4 == 0 ? $" {c}" : c.ToString())
        );
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
        VM.OnPatientAdded -= _onPatientAdded;
        VM.OnShowMessage -= _onShowMessage;
        VM.OnShowWarning -= _onShowWarning;
        VM.OnShowError -= _onShowError;
        VM.OnShowConcurrencyError -= _onShowConcurrencyError;

        _cts?.Dispose();
    }
}