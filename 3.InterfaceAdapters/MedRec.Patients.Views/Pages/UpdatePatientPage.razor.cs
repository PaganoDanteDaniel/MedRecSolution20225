using MedRec.Patients.ViewModels.VM;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MedRec.Patients.Views.Pages;
public partial class UpdatePatientPage
{
    [Inject] public UpdatePatientVM VM { get; set; }
    [Parameter] public int MaxPageButton { get; set; } = 9;
    [Parameter] public Guid PatientId { get; set; }

    private string footerMessage;
    private void OnMessageFooterChange(string message)
    {
        footerMessage = message;
    }

    ErrorBoundary ErrorBoundaryRef;

    void Recover()
    {
        ErrorBoundaryRef?.Recover();
    }
}