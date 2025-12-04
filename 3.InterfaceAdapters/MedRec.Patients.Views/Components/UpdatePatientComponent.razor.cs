using MedRec.CommonComponents.Views;
using MedRec.Patients.ViewModels.Models;
using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Resources;
using MedRec.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace MedRec.Patients.Views.Components;
public partial class UpdatePatientComponent
{
    #region Fields

    private bool _showHealthCompanyList;
    private bool _navigateAfterClose = false;
    private string _navigationUrl = "/";
    private bool _showModal;
    private string _modalTitle = "Mensaje del sistema";
    private ModalType _modalType = ModalType.MessageInfo;
    private UpdatePatientModel _originalModel;
    private string _healthInsuranceCardDisplay = string.Empty;

    private EditContext _editContext;

    private CancellationTokenSource _cts;

    private ErrorBoundary ErrorBoundaryRef;
    #endregion

    private bool HasChanges => !DeepEquals(_originalModel, VM.Model);

    #region Injected Services
    [Inject] public NavigationManager Navigation { get; set; }
    #endregion

    #region Parameters
    [Parameter] public UpdatePatientVM VM { get; set; }
    [Parameter] public Guid PatientId { get; set; }

    #endregion

    #region Methods
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

    protected override async Task OnInitializedAsync()
    {

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        _editContext = new EditContext(VM.Model);
        // 1. Primero carga los datos
        await VM.GetPatient(PatientId, _cts.Token);
        _originalModel = VM.Model.Clone();

        // 2. Luego crea el EditContext con el modelo YA CARGADO
        VM.OnShowMessage += () => ShowModalMessage("Información", ModalType.MessageInfo);
        VM.OnShowWarning += () => ShowModalMessage("Advertencia", ModalType.MessageWarning);
        VM.OnShowError += () => ShowModalMessage("Error", ModalType.MessageError);
        VM.OnShowConcurrencyError += () => ShowModalMessage("Conflicto de concurrencia", ModalType.MessageError);

        VM.OnPatientUpdated += () => ShowModalMessageAndNavigate("Actualización exitosa...", ModalType.MessageSuccess, "/");
        StateHasChanged();

    }

    private void ShowModalMessageAndNavigate(string title, ModalType type, string navigationUrl)
    {
        VM.InformationMessage = UpdatePatientMessages.UpdatePatientTemplate;
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
    private async Task UpdatePatient()
    {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        await VM.UpdatePatient(_cts.Token);
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
    private void OpenHealthCompanyList()
    {
        _showHealthCompanyList = true;
    }
    private void OnHealthCompanySelected((Guid id, string name) selectedCompany)
    {
        VM.Model.HealthInsuranceCompanyId = selectedCompany.id;
        VM.Model.SelectedHealthCompanyName = selectedCompany.name;
        _showHealthCompanyList = false;
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
    private void ClearForm()
    {
        VM.Model = new();
    }
    private void ExitForm()
    {
        Navigation.NavigateTo("/", true);
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
            _ => propertyName // fallback
        };
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