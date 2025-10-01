using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MedRec.Patients.Views.Components;
public partial class CreatePatientComponent : IDisposable
{
    #region Fields
    private bool _showHealthCompanyList;
    private bool _showModal = false;
    private bool _showErrorModal = false;
    private bool _showWarning = false;
    #endregion

    #region Events handler
    [Parameter] public EventCallback OnPatientAdded { get; set; }
    #endregion

    #region Inject Services
    [Inject] public NavigationManager Navigation { get; set; }
    #endregion

    #region Parameters
    [Parameter] public CreatePatientVM VM { get; set; }
    #endregion

    protected override void OnInitialized()
    {
        VM.OnPatientAdded += PatientAdded;
        VM.OnShowMessage += ShowError;
        VM.OnShowWarning += ShowWarning;
    }
    private void PatientAdded()
    {
        OnPatientAdded.InvokeAsync();
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
    private void OnAccept()
    {
        _showModal = false;
        _showWarning = false;
    }
    private void ExitForm(MouseEventArgs e)
    {
        Navigation.NavigateTo("/", true);
    }
    private string CalculateAge()
    {
        var today = DateTime.Today;
        var birthDate = VM.Model.DateOfBirth;
        if (!birthDate.HasValue)
            return string.Empty; // o algún mensaje por defecto, p.ej. "Fecha no definida"
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
        return (string.Format(CreatePatientMessages.AgeOfPatientTemplate, year.ToString(), month.ToString()));
    }

    public void Dispose()
    {
        VM.OnShowMessage -= ShowError;
        VM.OnPatientAdded -= PatientAdded;
        VM.OnShowWarning -= ShowWarning;
    }
}