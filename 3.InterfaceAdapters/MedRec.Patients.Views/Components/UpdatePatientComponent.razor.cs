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
    private bool _showModal = false;
    private bool _showErrorModal = false;
    private bool _showWarning = false;
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


        VM.OnPatientUpdated += HandleUpdateSuccess;
        VM.OnShowMessage += ShowError;
        VM.OnShowWarning += ShowWarning;

        StateHasChanged();

    }

    private async Task UpdatePatient()
    {
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        await VM.UpdatePatient(_cts.Token);
    }

    private async void HandleUpdateSuccess()
    {
        //await FooterMessageParameterChanged.InvokeAsync(FooterMessage);
        _showModal = true;
    }
    private void ShowWarning()
    {
        _showWarning = true;
        StateHasChanged();
    }
    private void ShowError()
    {
        _showErrorModal = true;
        StateHasChanged(); // fuerza actualización de UI
    }
    private void OnAccept()
    {
        _showModal = false;
        Navigation.NavigateTo("/", true);
    }
    private void Dispose()
    {
        _showModal = false;
        VM.OnPatientUpdated -= HandleUpdateSuccess;
    }
    private string CalculateAge()
    {
        var today = DateTime.Today;
        var birthDate = VM.Model.DateOfBirth;
        if (!birthDate.HasValue)
            return "Fecha no definida";

        // Calculating the age
        int year = today.Year - birthDate.Value.Year;
        int month = today.Month - birthDate.Value.Month;
        int day = today.Day - birthDate.Value.Day;

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
        return (string.Format(UpdatePatientMessages.AgeOfPatientTemplate, year.ToString(), month.ToString()));
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
    void Recover()
    {
        ErrorBoundaryRef?.Recover();
    }
    private void ExitForm()
    {
        Navigation.NavigateTo("/", true);
    }
    #endregion

}