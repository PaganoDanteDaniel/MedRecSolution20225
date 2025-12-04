using MedRec.CommonComponents.Views;
using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace MedRec.Patients.Views.Components;
public partial class CreatePatientComponent : IDisposable
{
    #region Fields
    private EditContext _editContext;
    private bool _showHealthCompanyList;

    private bool _navigateAfterClose = false;
    private string _navigationUrl = "/";
    private bool _showModal;
    private string _modalTitle = "Mensaje del sistema";
    private ModalType _modalType = ModalType.MessageInfo;


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

        _onPatientAdded = () => ShowModalMessageAndNavigate("Registro exitoso...", ModalType.MessageSuccess, "/");
        _onShowMessage = () => ShowModalMessage("Información", ModalType.MessageInfo);
        _onShowWarning = () => ShowModalMessage("Advertencia", ModalType.MessageWarning);
        _onShowError = () => ShowModalMessage("Error", ModalType.MessageError);
        _onShowConcurrencyError = () => ShowModalMessage("Conflicto de concurrencia", ModalType.MessageError);

        VM.OnShowMessage += _onShowMessage;
        VM.OnShowWarning += _onShowWarning;
        VM.OnShowError += _onShowError;
        VM.OnShowConcurrencyError += _onShowConcurrencyError;

        VM.OnPatientAdded += _onPatientAdded;
        StateHasChanged();
    }

    private void ShowModalMessageAndNavigate(string title, ModalType type, string navigationUrl)
    {
        VM.InformationMessage = CreatePatientMessages.PatientAddedMessage;
        _modalTitle = title;
        _modalType = type;
        _showModal = true;
        _navigateAfterClose = true;
        _navigationUrl = navigationUrl;
        InvokeAsync(StateHasChanged);
    }
    private void ShowModalMessage(string title, ModalType type)
    {
        _modalTitle = title;
        _modalType = type;
        _showModal = true;
        _navigateAfterClose = false;
        InvokeAsync(StateHasChanged);
    }
    private void CloseModal()
    {
        _showModal = false;

        if (_navigateAfterClose)
        {
            _navigateAfterClose = false;
            Navigation.NavigateTo(_navigationUrl, true);
        }
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