using MedRec.CommonComponents.Views;
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
    private bool ShowModal;
    private string ModalTitle = "Mensaje del sistema";
    private ModalType ModalType = ModalType.MessageInfo;

    private EditContext _editContext;

    private CancellationTokenSource _cts;

    private ErrorBoundary ErrorBoundaryRef;
    #endregion

    #region Injected Services
    [Inject] public NavigationManager Navigation { get; set; }
    #endregion

    #region Parameters
    [Parameter] public UpdatePatientVM VM { get; set; }
    [Parameter] public Guid PatientId { get; set; }

    #endregion

    #region Methods
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

        // 2. Luego crea el EditContext con el modelo YA CARGADO

        VM.OnShowMessage += () => ShowModalMessage("Información", ModalType.MessageInfo);
        VM.OnShowWarning += () => ShowModalMessage("Advertencia", ModalType.MessageWarning);
        VM.OnShowError += () => ShowModalMessage("Error", ModalType.MessageError);
        VM.OnShowConcurrencyError += () => ShowModalMessage("Conflicto de concurrencia", ModalType.MessageError);

        VM.OnPatientUpdated += () => ShowModalMessage("Actualización exisota...", ModalType.MessageSuccess);
        StateHasChanged();

    }

    private void ShowModalMessage(string title, ModalType type)
    {
        ModalTitle = title;
        ModalType = type;
        ShowModal = true;
        InvokeAsync(StateHasChanged);
    }
    private void CloseModal()
    {
        ShowModal = false;
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