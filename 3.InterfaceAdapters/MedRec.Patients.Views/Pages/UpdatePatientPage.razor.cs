using MedRec.CommonComponents.Views;
using MedRec.CommonComponents.Views.Page;
using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Components;
using Microsoft.AspNetCore.Components;

namespace MedRec.Patients.Views.Pages;
public partial class UpdatePatientPage
{
    private UpdatePatientVM VM => Service;
    [Inject] public NavigationManager Navigation { get; set; }
    [Parameter] public Guid PatientId { get; set; }

    #region === NUEVAS PROPIEDADES PARA EL NUEVO MODAL ===

    private bool ModalVisible;
    private ModalType ModalType = ModalType.MessageInfo;
    private string ModalTitle = "Mensaje";
    private string ModalMessage = "";
    private RenderFragment? ModalBody;
    private bool CloseOnOverlayClick = true;
    private bool ShowOk = false;
    private bool ShowCancel = false;
    private bool ShowDelete = false;
    private bool ShowRetry = false;

    #endregion

    #region Fields
    private string footerMessage = "MedRec Software de gestión médica";
    private bool _showModal = false;
    private PageShell pageShellRef;
    private bool _isLoading = false;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        if (PatientId == Guid.Empty)
        {
            await OpenAssignPatientModal();
        }
    }
    private Task OnBackPressed(int _)
    {
        Navigation.NavigateTo("/patient-control");
        return Task.CompletedTask;
    }
    private Task OpenAssignPatientModal()
    {
        CloseOnOverlayClick = false;
        ModalType = ModalType.NormalContent;
        ModalTitle = "Selector de paciente";
        ShowOk = false;
        ShowCancel = false;
        ShowDelete = false;
        ShowRetry = false;
        ModalBody = builder =>
        {
            builder.OpenComponent<ListPatientsComponent>(0);
            builder.AddAttribute(1, nameof(ListPatientsComponent.MaxPageButton), 3);
            builder.AddAttribute(2, nameof(ListPatientsComponent.WithHeight), false);
            builder.AddAttribute(3, nameof(ListPatientsComponent.OnPatientSelected),
                EventCallback.Factory.Create<(Guid, string, string)>(this, OnPatientSelected));
            builder.AddAttribute(4, nameof(ListPatientsComponent.ShowActionsColumn), false);
            builder.CloseComponent();
        };



        _showModal = true; // Primero selector de paciente
        return Task.CompletedTask;
    }
    private void OnPatientSelected((Guid idSelectedPatient, string _, string __) selectedPatient)
    {
        _showModal = false;
        // Redirigir a la misma página pero con el ID en la ruta -> asegura un estado limpio
        Navigation.NavigateTo($"/patient/update/{selectedPatient.idSelectedPatient}");
    }
}