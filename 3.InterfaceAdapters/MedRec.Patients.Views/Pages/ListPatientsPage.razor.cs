using MedRec.CommonComponents.Views.Page;
using Microsoft.AspNetCore.Components;

namespace MedRec.Patients.Views.Pages;
public partial class ListPatientsPage
{
    [Inject] NavigationManager Navigation { get; set; }
    [Parameter] public int MaxPageButton { get; set; } = 9;

    private string footerMessage = "MedRec Software de gestión médica";
    private PageShell pageShellRef;
    private bool _isLoading = false;

    private Task OnBackPressed(int _)
    {
        Navigation.NavigateTo("/patient-control");
        return Task.CompletedTask;
    }
    private void SetLoading(bool loading)
    {
        _isLoading = loading;
        InvokeAsync(StateHasChanged);
    }
    private void OnMessageFooterChange(string title)
    {
        footerMessage = title;
    }

    private Task PatientUpdate(Guid patientId)
    {
        Navigation.NavigateTo($"/patient/update/{patientId}", true);
        return Task.CompletedTask;
    }

}