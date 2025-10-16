
using MedRec.MedicalVisit.ViewModels.VM;
using MedRec.MedicalVisit.Views.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;


namespace MedRec.MedicalVisit.Views.Pages;

public partial class CreateMedicalVisitPage
{
    [Inject] public CreateMedicalVisitVM VM { get; set; }
    [Parameter] public Guid PatientId { get; set; }
    [Parameter] public Guid? VisitId { get; set; }
    [Parameter] public string PatientName { get; set; }


    private string footerMessage;
    private bool IsReadOnly => VisitId.HasValue;
    private string Title { get; set; }
    protected override async Task OnParametersSetAsync()
    {
        if (IsReadOnly && VisitId.HasValue)
        {
            // Modo Vista: Cargar datos de la visita existente
            await VM.LoadVisitAsync(VisitId.Value);
            Title = $"{PatientName} | Fecha de visita {VM.Model.VisitDate:dd/MM/yyyy}";

        }
        else
        {
            // Modo Creación: Inicializar para una nueva visita
            await VM.InitializeNewVisit(PatientId);
            Title = string.Format(MedicalVisitMessages.CreateMedicalVisitTitleTemplate, PatientName);
        }
    }
    private void OnMessageFooterChange(string title)
    {
        footerMessage = title;
    }
    ErrorBoundary ErrorBoundaryRef;

    void Recover()
    {
        ErrorBoundaryRef?.Recover();
    }
}