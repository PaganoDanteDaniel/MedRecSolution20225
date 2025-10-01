using MedRec.Patients.ViewModels.VM;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MedRec.Patients.Views.Pages;
public partial class CreatePatientPage
{
    #region Injects services
    [Inject] public CreatePatientVM VM { get; set; }
    [Inject] public NavigationManager Navigation { get; set; }
    #endregion

    #region Fields
    ErrorBoundary ErrorBoundaryRef;
    string footerMessage = "";
    #endregion

    #region Methods
    void Recover()
    {
        ErrorBoundaryRef?.Recover();
    }
    #endregion
}