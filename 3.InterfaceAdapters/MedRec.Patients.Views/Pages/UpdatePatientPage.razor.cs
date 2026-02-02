using MedRec.CommonComponents.Views.Page;
using MedRec.Patients.ViewModels.VM;
using Microsoft.AspNetCore.Components;

namespace MedRec.Patients.Views.Pages;
public partial class UpdatePatientPage
{
    private UpdatePatientVM VM => Service;
    [Inject] public NavigationManager Navigation { get; set; }
    [Parameter] public Guid PatientId { get; set; }

    #region Fields
    private string footerMessage = "MedRec Software de gestión médica";

    private PageShell pageShellRef;
    private bool _isLoading = false;
    #endregion
    private Task OnBackPressed(int _)
    {
        Navigation.NavigateTo("/patient-control");
        return Task.CompletedTask;
    }
}