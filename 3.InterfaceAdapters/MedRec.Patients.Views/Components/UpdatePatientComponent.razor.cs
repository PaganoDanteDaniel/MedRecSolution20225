using MedRec.CommonComponents.Views;
using MedRec.Patients.ViewModels.Models;
using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Resources;
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

    private string CalculateAge()
    {
        var today = DateTime.Today;
        var birthDate = VM.Model.DateOfBirth;

        // Calculating the age
        int year = today.Year - birthDate.Year;
        int month = today.Month - birthDate.Month;
        int day = today.Day - birthDate.Day;

        // Ajuste si el día actual es menor que el d�a de nacimiento
        if (day < 0)
        {
            month--;
            day += DateTime.DaysInMonth(today.Year, today.Month - 1);
        }

        // Ajuste si el mes actual es menor que el mes de nacimiento
        if (month < 0)
        {
            year--;
            month += 12;
        }
        return (string.Format(CreatePatientMessages.AgeOfPatientTemplate, year.ToString(), month.ToString()));
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
    private void ClearForm()
    {
        VM.Model = new();
    }
    private void ExitForm()
    {
        Navigation.NavigateTo("/", true);
    }
    #endregion

}