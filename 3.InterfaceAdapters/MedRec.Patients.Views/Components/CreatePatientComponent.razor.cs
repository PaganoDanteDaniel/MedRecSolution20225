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
    private bool _showErrorModal = false;
    private bool _showWarning = false;

    private bool _navigateAfterClose = false;
    private string _navigationUrl = "/";
    private bool _showModal;
    private string _modalTitle = "Mensaje del sistema";
    private ModalType _modalType = ModalType.MessageInfo;


    private CancellationTokenSource _cts;
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

    protected override void OnInitialized()
    {
        _editContext = new EditContext(VM.Model);

        _onPatientAdded = () => ShowModalMessageAndNavigate("Registro exitos...", ModalType.MessageSuccess, "/");
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

    private void OnFieldChanged<T>(Expression<Func<T>> propertyExpression)
    {
        var fieldIdentifier = FieldIdentifier.Create(propertyExpression);

        _editContext?.NotifyFieldChanged(fieldIdentifier);
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