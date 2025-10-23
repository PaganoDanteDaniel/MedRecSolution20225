using MedRec.Patients.ViewModels.VM;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MedRec.Patients.Views.Pages;
public partial class ListPatientsPage
{

    [Inject] public PatientsListVM Model { get; set; }
    [Inject] NavigationManager Navigation { get; set; }
    [Parameter] public int MaxPageButton { get; set; } = 9;

    private string footerMessage;

    private void OnMessageFooterChange(string title)
    {
        footerMessage = title;
    }
    protected override void OnInitialized()
    {

    }

    private void PatientUpdate(Guid patientId)
    {

        Navigation.NavigateTo($"/patient/update/{patientId}", true);
    }


    ErrorBoundary ErrorBoundaryRef;

    void Recover()
    {
        ErrorBoundaryRef?.Recover();
    }
}