using MedRec.Entity.Extensions;
using MedRec.MedicalVisit.ViewModels.VM;
using MedRec.MedicalVisit.Views.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;


namespace MedRec.MedicalVisit.Views.Pages;

public partial class CreateMedicalVisitPage
{
    [Inject] public CreateMedicalVisitVM VM { get; set; }

    public Guid? VisitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; }
    public DateTime DateOfBirth { get; set; }

    private string footerMessage;
    private bool IsReadOnly => VisitId.HasValue;
    private string Title { get; set; }

    protected override void OnInitialized()
    {
        // Llamada a una función auxiliar para leer los parámetros de la URL
        ReadParameterFromUrl();

        // **IMPORTANTE:** Aquí, si PatientId sigue siendo Guid.Empty, es donde ocurre el error.
        // Asegúrate de que PatientId no sea Guid.Empty después de esta función si
        // la validación que genera la excepción está aquí o en un componente hijo.

        base.OnInitialized();
    }

    // Función auxiliar para leer los valores de la Query String
    private void ReadParameterFromUrl()
    {
        // 1. Obtener la URI actual
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);

        // 2. Intentar leer el 'id'
        if (Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("id", out var idValue) &&
            Guid.TryParse(idValue, out var patientGuid))
        {
            PatientId = patientGuid;
        }
        if (Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("visitId", out var visitIdValue) &&
            Guid.TryParse(visitIdValue, out var visitId))
        {
            VisitId = visitId;
        }

        // 3. Intentar leer el 'nombre'
        if (Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("nombre", out var nombreValue))
        {
            PatientName = System.Web.HttpUtility.UrlDecode(nombreValue);
        }

        // 4. Intentar leer la 'fechaNac'
        if (Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("fechaNac", out var fechaValue) &&
            DateTime.TryParse(fechaValue, out var dob))
        {
            DateOfBirth = dob;
        }

        // 5. Verificar si el ID se cargó correctamente
        if (PatientId == Guid.Empty)
        {
            // Opcional: Manejar el caso de ID faltante
            Console.WriteLine("Error: PatientId no se pudo cargar de la URL.");
            // Podrías redirigir o mostrar un mensaje de error aquí.
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (IsReadOnly && VisitId.HasValue)
        {
            // Modo Vista: Cargar datos de la visita existente
            await VM.LoadVisitAsync(VisitId.Value);
            Title = $"{PatientName} | EDAD {DateOfBirth.CalculateAge()} AÑOS | Fecha de visita {VM.Model.VisitDate:dd/MM/yyyy}";

        }
        else
        {
            // Modo Creación: Inicializar para una nueva visita
            await VM.InitializeNewVisit(PatientId);
            Title = string.Format(MedicalVisitMessages.CreateMedicalVisitTitleTemplate, PatientName, DateOfBirth.CalculateAge());
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