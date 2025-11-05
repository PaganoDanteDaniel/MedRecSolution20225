using MedRec.MedicalAppointments.ViewModels.VM;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace MedRec.MedicalAppointments.Views.Components;
public partial class WeeklyScheduleComponent(WeeklyScheduleViewModel VM) : ComponentBase
{
    // Definiciones de estado (equivalente a las variables let/const de JavaScript)
    private DateTime fechaBase = DateTime.Today;
    private string turno = ""; // agregar si se desea separar agenda de mañana o tarde. "MAÑANA";
    private Dictionary<string, Turno> agendaData = new();
    private bool modoMoverActivo = false;
    private string? claveOrigen = null;

    private bool _showListPatients = false;

    private (DateTime Start, DateTime End)? SelectedWeek;

    private bool mostrarModalPatient = false;
    private bool mostrarModal = false;
    private string claveModal = "";
    private string pacienteTemp = "";
    private string motivoTemp = "";
    private enum ModalTipo { Ninguno, Asignar, Gestion }
    private ModalTipo modalTipo = ModalTipo.Ninguno;

    // Propiedades computadas (equivalente a la lógica de actualización en JS)
    private List<DateTime> diasSemana => Enumerable.Range(0, 5)
        .Select(i =>
        {
            // Calcula el lunes de la semana de fechaBase
            var lunes = fechaBase.AddDays(1 - (int)fechaBase.DayOfWeek);
            if (lunes.DayOfWeek == DayOfWeek.Sunday) lunes = lunes.AddDays(-6);
            return lunes.AddDays(i);
        })
        .ToList();

    // Genera horarios pos separados para la mañana y la tarde.
    //private List<string> horarios => turno == "MAÑANA"
    //    ? GenerarIntervalos("09:30", "12:30")
    //    : GenerarIntervalos("17:30", "20:30");

    // Genera todos los horarios del día.
    private List<string> horarios =>
        GenerarIntervalosDia("09:30", "12:30", "17:30", "20:30");

    private string Leyenda => $"AGENDA MES DE: {Capitalizar(fechaBase.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-ES")))}";
    // agregar al final de la línea anterior si se desea separar agende de mañana y tarde "- {Capitalizar(turno)}";

    private string BotonTurnoTexto => turno == "MAÑANA" ? "TARDE" : "MAÑANA";

    // Método de inicialización (equivalente al JS que se ejecuta al cargar)
    protected override void OnInitialized()
    {

    }

    private void AlternarTurno()
    {
        turno = turno == "MAÑANA" ? "TARDE" : "MAÑANA";

        // modoMoverActivo = false;
        // claveOrigen = null;
    }

    private void CeldaClic(string clave, bool esPasado, bool tieneTurno)
    {
        if (esPasado) return;

        if (modoMoverActivo)
        {
            if (clave == claveOrigen)
            {
                CancelarModoMover();
            }
            else
            {
                if (!tieneTurno)
                {
                    // Mover turno
                    if (agendaData.TryGetValue(claveOrigen!, out var turnoOrigen))
                    {
                        agendaData[clave] = turnoOrigen;
                        agendaData.Remove(claveOrigen!);
                    }
                    CancelarModoMover();
                }


            }
        }
        else
        {
            if (tieneTurno)
            {
                AbrirModalGestion(clave);
            }
            else
            {
                AbrirModalAsignar(clave);
            }
        }
    }

    private void AbrirModalAsignar(string clave)
    {
        claveModal = clave;
        pacienteTemp = ""; // Limpiar campos del modal
        motivoTemp = "";
        modalTipo = ModalTipo.Asignar;
        mostrarModalPatient = true;
    }

    private void AbrirModalGestion(string clave)
    {
        claveModal = clave;
        modalTipo = ModalTipo.Gestion;
        mostrarModal = true;
    }

    private void GuardarTurno(string clave)
    {
        if (!string.IsNullOrWhiteSpace(pacienteTemp) && !string.IsNullOrWhiteSpace(motivoTemp))
        {
            agendaData[clave] = new Turno { Paciente = pacienteTemp, Motivo = motivoTemp };
            CerrarModal();
        }
    }

    private void CancelarTurno(string clave)
    {
        agendaData.Remove(clave);
        CerrarModal();
    }

    private void ActivarModoMover(string clave)
    {
        modoMoverActivo = true;
        claveOrigen = clave;
        CerrarModal();
    }

    private void CancelarModoMover()
    {
        modoMoverActivo = false;
        claveOrigen = null;
    }

    private void CerrarModal()
    {
        mostrarModal = false;
        modalTipo = ModalTipo.Ninguno;
    }

    private void CerrarModalExterior()
    {
        if (mostrarModal)
        {
            CerrarModal();
        }
    }

    // Genera intervalos de horarios según turno Mañana o Tarde.
    private List<string> GenerarIntervalos(string inicio, string fin)
    {
        var resultado = new List<string>();
        var horaInicio = TimeSpan.Parse(inicio);
        var horaFin = TimeSpan.Parse(fin);
        var actual = horaInicio;

        while (actual < horaFin)
        {
            resultado.Add(actual.ToString(@"hh\:mm"));
            actual = actual.Add(TimeSpan.FromMinutes(15));
        }

        return resultado;
    }
    private List<string> GenerarIntervalosDia(string inicioMañana, string finMañana, string inicioTarde, string finTarde)
    {
        var resultado = new List<string>();
        var horaInicioMañana = TimeSpan.Parse(inicioMañana);
        var horaFinMañana = TimeSpan.Parse(finMañana);
        var horaInicioTarde = TimeSpan.Parse(inicioTarde);
        var horaFinTarde = TimeSpan.Parse(finTarde);
        var actual = horaInicioMañana;

        while (actual < horaFinMañana)
        {
            resultado.Add(actual.ToString(@"hh\:mm"));
            actual = actual.Add(TimeSpan.FromMinutes(15));
        }
        actual = horaInicioTarde;
        while (actual < horaFinTarde)
        {
            resultado.Add(actual.ToString(@"hh\:mm"));
            actual = actual.Add(TimeSpan.FromMinutes(15));
        }

        return resultado;
    }
    private string Capitalizar(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return texto;
        // La sobrecarga ToTitleCase puede ser mejor para capitalizar
        return CultureInfo.CurrentCulture.TextInfo.ToUpper(texto);
    }

    // Clase de modelo para los datos del turno
    public class Turno
    {
        public string Paciente { get; set; } = "";
        public string Motivo { get; set; } = "";
    }

    private void OnWeekSelected((DateTime Start, DateTime End) week)
    {
        SelectedWeek = week;

        // Si necesitas mantener compatibilidad con lógica anterior que usa un solo día,
        // puedes usar, por ejemplo, el lunes de la semana:
        fechaBase = week.Start; // o week.End, según tu lógica
        StateHasChanged();
        // Ejemplo: cargar turnos médicos
        //LoadAppointments(week.Start, week.End);
    }

    private async Task OnPatientSelected((Guid idSelectedPatient, string nameSelectedPatient, string phoneSelectedPatient) selectedPatient)
    {
        agendaData[claveModal] = new Turno { Paciente = selectedPatient.nameSelectedPatient, Motivo = selectedPatient.phoneSelectedPatient };
        CerrarModal();
        mostrarModalPatient = false;
        //VM.Model.HealthInsuranceCompanyId = selectedCompany.id;
        //VM.Model.SelectedHealthCompanyName = selectedCompany.nameselectedCompany;
        //_showHealthCompanyList = false; // Cerrar el modal
        //await Task.CompletedTask;
    }
}