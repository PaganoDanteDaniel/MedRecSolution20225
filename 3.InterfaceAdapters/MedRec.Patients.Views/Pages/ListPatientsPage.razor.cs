using MedRec.Patients.ViewModels.VM;
using MedRec.Patients.Views.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MedRec.Patients.Views.Pages;
public partial class ListPatientsPage
{

    [Inject] public PatientsListVM Model { get; set; }
    [Inject] NavigationManager Navigation { get; set; }
    [Parameter] public int MaxPageButton { get; set; } = 9;
    private string footerMessage;
    private Guid patientId;
    private string name;
    private bool showPatientDelete;

    private TaskCompletionSource<bool> deleteConfirmationTcs;
    private void OnMessageFooterChange(string title)
    {
        footerMessage = title;
    }
    protected override void OnInitialized()
    {
        Model.OnPatientDeleted += OnDelete;
    }

    private void OnDelete()
    {
        if (Model.InformationMessage != null)
        {
            footerMessage = string.Format(ListPatientsMessages.ErrorDeletePatientMessageTemplate, name);
        }
        else
        {
            footerMessage = string.Format(ListPatientsMessages.DeletedPatientMessageTemplate, name);
        }

        StateHasChanged();

    }

    private async Task PatientDelete((Guid patientId, string name) patient)
    {
        patientId = patient.patientId;
        name = patient.name;

        showPatientDelete = true;
        deleteConfirmationTcs = new TaskCompletionSource<bool>(); // Crear una tarea que se completará cuando el usuario acepte o rechace

        // Esperar hasta que el usuario interactúe con el modal
        bool isConfirmed = await deleteConfirmationTcs.Task;
        if (isConfirmed)
        {
            await Model.DeleteAsync(patientId);
        }
    }

    private void PatientUpdate(Guid patientId)
    {

        Navigation.NavigateTo($"/patient/update/{patientId}", true);
    }

    private void OnAcceptDelete()
    {
        showPatientDelete = false;
        // Completar la tarea cuando el usuario acepta la eliminación
        deleteConfirmationTcs?.SetResult(true);
    }

    private void OnClosePatientDelete(bool value)
    {
        showPatientDelete = value;
        // Completar la tarea con `false` si el usuario cierra el modal sin aceptar
        deleteConfirmationTcs?.SetResult(false);
    }

    ErrorBoundary ErrorBoundaryRef;

    void Recover()
    {
        ErrorBoundaryRef?.Recover();
    }
}