using MedRec.MedicalAppointments.ViewModels.Models;
using MedRec.MedicalAppointments.ViewModels.VM;
using Microsoft.JSInterop;
using System.Globalization;

namespace MedRec.MedicalAppointments.Views.Components;
public partial class WeeklyScheduleComonet
{
    private IJSObjectReference? module;
    private WeeklyScheduleViewModelOrchestrator VM => Service;
    // Estado
    private DateTime fechaBase = DateTime.Today;
    private string turno = "";
    private bool activeMoveMode = false;
    private bool activeReassignMode = false;

    // Modelo de grilla
    private readonly List<ScheduleRow> _rows = new();

    // Selecciones y estado de UI
    private (DateTime Start, DateTime End)? SelectedWeek;
    private ScheduleCell? celdaOrigen;
    private ScheduleCell? celdaSeleccionada;

    private bool mostrarModalPatient = false;
    private bool mostrarModal = false;
    private string pacienteTemp = "";
    private string motivoTemp = "";

    private enum ModalTipo { Ninguno, Asignar, Gestion }
    private ModalTipo modalTipo = ModalTipo.Ninguno;

    // Días de la semana (Lun-Vie) a partir de fechaBase
    private List<DateTime> diasSemana => Enumerable.Range(0, 5)
        .Select(i =>
        {
            var lunes = fechaBase.AddDays(1 - (int)fechaBase.DayOfWeek);
            if (lunes.DayOfWeek == DayOfWeek.Sunday) lunes = lunes.AddDays(-6);
            return lunes.Date.AddDays(i);
        })
        .ToList();

    // Intervalos de día (mañana y tarde)
    private List<string> horarios => GenerarIntervalosDia("09:30", "12:30", "17:30", "20:30");

    private string Leyenda => $"AGENDA MES DE: {Capitalizar(fechaBase.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-ES")))}";

    private string BotonTurnoTexto => turno == "MAÑANA" ? "TARDE" : "MAÑANA";
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Intentar sincronizar anchos
            try
            {
                await JSRuntime.InvokeVoidAsync("syncTableWidths");
            }
            catch (Exception ex)
            {
                // Opcional: log
                Console.WriteLine($"Error al sincronizar: {ex.Message}");
            }
        }
    }
    protected override async Task OnInitializedAsync()
    {
        // Opcional: precargar semana actual
        var lunes = fechaBase.AddDays(1 - (int)fechaBase.DayOfWeek);
        if (fechaBase.DayOfWeek == DayOfWeek.Sunday) // corrección del caso domingo
            lunes = fechaBase.AddDays(-6);

        var start = lunes.Date;
        var end = start.AddDays(4).Date.AddHours(23).AddMinutes(59);
        SelectedWeek = (start, end);

        try
        {
            await LoadWeekAsync(start, end);
        }
        catch (Exception ex)
        {
            // TODO: inyecta ILogger<WeeklyScheduleComonet> y registra el error
            // _logger.LogError(ex, "Error cargando la semana");
            throw;
        }
    }

    private void AlternarTurno()
    {
        turno = turno == "MAÑANA" ? "TARDE" : "MAÑANA";
    }

    // Construir grilla en base a la semana seleccionada y las citas del VM
    private async Task LoadWeekAsync(DateTime start, DateTime end)
    {
        var citas = await VM.GetAppointments(start, end);
        //var citas = new List<Appointment> { };
        var mapa = citas?
            .Where(a => a is not null)
            .ToDictionary(a => a.DateTime.ToString("yyyy-MM-dd HH:mm")) ?? new Dictionary<string, Appointment>();

        _rows.Clear();

        foreach (var hora in horarios)
        {
            var row = new ScheduleRow { Time = hora };

            foreach (var dia in diasSemana)
            {
                var dt = DateTime.ParseExact($"{dia:yyyy-MM-dd} {hora}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                var celda = new ScheduleCell { DateTime = dt };

                if (mapa.TryGetValue(celda.Key, out var appt) && !appt.IsDeleted)
                {
                    celda.Appointment = appt;
                }

                row.Cells.Add(celda);
            }

            _rows.Add(row);
        }

        StateHasChanged();
    }

    // CSS por estado de celda
    private string GetCssClass(ScheduleCell celda)
    {
        if (celda.IsPast) return "pasado";
        if (activeMoveMode && celdaOrigen is not null && celda.Key != celdaOrigen.Key && celda.Appointment is null) return "modo-mover";
        if (celda.Appointment is not null) return "asignado";
        return "";
    }

    private void CeldaClic(ScheduleCell celda)
    {

        if (celda.IsPast) return;

        if (activeMoveMode)
        {
            if (celdaOrigen is null)
            {
                CancelarModoMover();
                return;
            }

            if (ReferenceEquals(celda, celdaOrigen))
            {
                CancelarModoMover();
                return;
            }

            if (celda.Appointment is null && celdaOrigen.Appointment is not null)
            {
                // Mover shift
                var appt = celdaOrigen.Appointment;
                celdaOrigen.Appointment = null;
                appt.DateTime = celda.DateTime;
                celda.Appointment = appt;

                _ = VM.MoveAsync(appt); // Persistencia simple (Upsert futuro).
                CancelarModoMover();
                StateHasChanged();
            }
        }
        else
        {
            celdaSeleccionada = celda;
            if (celda.Appointment is not null)
            {
                AbrirModalGestion();
            }
            else
            {
                AbrirModalAsignar();
            }
        }
    }

    private void AbrirModalAsignar()
    {
        pacienteTemp = "";
        motivoTemp = "";
        activeReassignMode = true;
        //modalType = ModalType.Asignar;
        mostrarModalPatient = true; // Primero selector de paciente
    }

    private void AbrirModalGestion()
    {
        modalTipo = ModalTipo.Gestion;
        mostrarModal = true;
    }

    private void ActivarModoMover()
    {
        if (celdaSeleccionada?.Appointment is null) return;
        activeMoveMode = true;
        celdaOrigen = celdaSeleccionada;
        CerrarModal();
    }

    private void CancelarModoMover()
    {
        activeMoveMode = false;
        celdaOrigen = null;
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

    // Guardar desde modal manual (paciente/motivo tipeado)
    private async Task SaveAppointmentFromModal()
    {
        if (celdaSeleccionada is null) return;
        if (string.IsNullOrWhiteSpace(pacienteTemp) || string.IsNullOrWhiteSpace(motivoTemp)) return;

        var appt = celdaSeleccionada.Appointment ?? new Appointment
        {
            Id = Guid.NewGuid(),
            DateTime = celdaSeleccionada.DateTime,
            IsDeleted = true
        };

        appt.PatientName = pacienteTemp;
        appt.Reason = motivoTemp;

        celdaSeleccionada.Appointment = appt;
        await VM.SaveChange(appt);

        CerrarModal();
        StateHasChanged();
    }

    private async Task CancelarTurno()
    {
        if (celdaSeleccionada?.Appointment is null) return;

        // Marcado simple como cancelado en UI; persistencia mínima
        var appt = celdaSeleccionada.Appointment;
        appt.IsDeleted = true;
        celdaSeleccionada.Appointment = null;

        await VM.DeleteAsync(appt); // En un futuro: VM.CancelAppointment(appt.Id)
        CerrarModal();
        StateHasChanged();
    }

    // Callback del selector de semanas
    private void OnWeekSelected((DateTime Start, DateTime End) week)
    {
        SelectedWeek = week;
        fechaBase = week.Start;
        _ = LoadWeekAsync(week.Start, week.End);
    }

    // Callback del selector de pacientes
    private async Task OnPatientSelected((Guid idSelectedPatient, string nameSelectedPatient, string phoneSelectedPatient) selectedPatient)
    {
        mostrarModalPatient = false;

        if (celdaSeleccionada is null) return;

        var appt = celdaSeleccionada.Appointment ?? new Appointment
        {
            //Id = Guid.NewGuid(),
            DateTime = celdaSeleccionada.DateTime,
            ProfessionalId = Guid.Parse("C771A686-20DB-4DE3-A547-60B368B1FA98"),
            IsDeleted = true
        };

        appt.PatientId = selectedPatient.idSelectedPatient;
        appt.PatientName = selectedPatient.nameSelectedPatient;
        appt.Phone = selectedPatient.phoneSelectedPatient;
        appt.Reason = selectedPatient.phoneSelectedPatient; // Se mantiene compat: teléfono como motivo si no hay otro campo

        // detectar si es reasignar o nuevo.
        celdaSeleccionada.Appointment = appt;
        if (activeReassignMode)
        {
            mostrarModal = false;
            activeReassignMode = false;
            await VM.ReassignAsync(appt);
        }
        else
        {
            await VM.SaveChange(appt);
        }



        // Si se quiere además abrir el modal para editar motivo, descomentar:
        // modalType = ModalType.Asignar;
        // showModal = true;

        StateHasChanged();
    }

    // Utilidades
    private List<string> GenerarIntervalosDia(string inicioManana, string finManana, string inicioTarde, string finTarde)
    {
        var resultado = new List<string>();

        var hIniM = TimeSpan.Parse(inicioManana);
        var hFinM = TimeSpan.Parse(finManana);
        var hIniT = TimeSpan.Parse(inicioTarde);
        var hFinT = TimeSpan.Parse(finTarde);

        var actual = hIniM;
        while (actual < hFinM)
        {
            resultado.Add(actual.ToString(@"hh\:mm"));
            actual = actual.Add(TimeSpan.FromMinutes(15));
        }

        actual = hIniT;
        while (actual < hFinT)
        {
            resultado.Add(actual.ToString(@"hh\:mm"));
            actual = actual.Add(TimeSpan.FromMinutes(15));
        }

        return resultado;
    }

    private string Capitalizar(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return texto;
        return CultureInfo.CurrentCulture.TextInfo.ToUpper(texto);
    }
}