using MedRec.CommonComponents.Views;
using MedRec.MedicalAppointments.ViewModels.Models;
using MedRec.MedicalAppointments.ViewModels.VM;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace MedRec.MedicalAppointments.Views.Components;
public partial class AppointmentScheduleComponent
{
    private WeeklyScheduleViewModel VM => Service;

    private ElementReference patientReasonInputRef;
    private bool shouldFocus = false; // Bandera para controlar el foco

    private string shift = "";
    private bool activeMoveMode = false;
    private bool activeReassignMode = false;

    // Selecciones y estado de UI
    private (DateTime Start, DateTime End)? SelectedWeek;
    private ScheduleCell? sourceCell;
    private ScheduleCell? selectedCell;

    private bool showPatientModal = false;
    private bool showModal = false;
    private Appointment tempAppointment = new();

    private bool _navigateAfterClose = false;
    private string _navigationUrl = "/";
    private bool _showModal;
    private string _modalTitle = "Mensaje del sistema";
    private ModalType _modalType = ModalType.MessageInfo;

    private enum Modal_Type { Ninguno, Asignar, Gestion }
    private Modal_Type modalType = Modal_Type.Ninguno;
    private string Legend => $"AGENDA MES DE: {Capitalize(VM.DateBase.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-ES")))}";


    protected override async Task OnInitializedAsync()
    {
        // Opcional: precargar semana actual
        var lunes = VM.DateBase.AddDays(1 - (int)VM.DateBase.DayOfWeek);
        if (VM.DateBase.DayOfWeek == DayOfWeek.Sunday) // corrección del caso domingo
            lunes = VM.DateBase.AddDays(-6);

        var start = lunes.Date;
        var end = start.AddDays(4).Date.AddHours(23).AddMinutes(59);
        SelectedWeek = (start, end);

        // 2. Luego crea el EditContext con el modelo YA CARGADO
        VM.OnShowMessage += () => ShowModalMessage("Información", ModalType.MessageInfo);
        VM.OnShowWarning += () => ShowModalMessage("Advertencia", ModalType.MessageWarning);
        VM.OnShowError += () => ShowModalMessage("Error", ModalType.MessageError);
        VM.OnShowConcurrencyError += () => ShowModalMessage("Conflicto de concurrencia", ModalType.MessageError);

        VM.OnFinnishOperation += () => ShowModalMessageAndNavigate("Actualización exitosa...", ModalType.MessageSuccess, "/");
        StateHasChanged();
        try
        {
            await VM.LoadWeekAsync(start, end);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void ShowModalMessageAndNavigate(string title, ModalType type, string navigationUrl)
    {
        VM.InformationMessage = "ACTUALIZACIÓN EXITOSA";
        _modalTitle = title;
        _modalType = type;
        _showModal = true;
        _navigateAfterClose = true;
        _navigationUrl = navigationUrl;
        InvokeAsync(StateHasChanged);
    }
    private void ShowModalMessage(string title, ModalType type)
    {
        _modalTitle = title;
        _modalType = type;
        _showModal = true;
        _navigateAfterClose = false;
        InvokeAsync(StateHasChanged);
    }

    private void ToggleShift()
    {
        shift = shift == "MAÑANA" ? "TARDE" : "MAÑANA";
    }

    // CSS por estado de celda
    private string GetCssClass(ScheduleCell celda)
    {
        if (celda.IsPast) return "pasado";
        if (activeMoveMode && sourceCell is not null && celda.Key != sourceCell.Key && celda.Appointment is null) return "modo-mover";
        if (celda.Appointment is not null) return "asignado";
        return "";
    }
    private void CeldaClic(ScheduleCell celda)
    {

        if (celda.IsPast) return;

        if (activeMoveMode)
        {
            if (sourceCell is null)
            {
                CancelMoveMode();
                return;
            }

            if (ReferenceEquals(celda, sourceCell))
            {
                CancelMoveMode();
                return;
            }

            if (celda.Appointment is null && sourceCell.Appointment is not null)
            {
                // Mover shift
                var appt = sourceCell.Appointment;
                sourceCell.Appointment = null;
                appt.DateTime = celda.DateTime;
                celda.Appointment = appt;

                _ = VM.MoveAsync(appt); // Persistencia simple (Upsert futuro).
                CancelMoveMode();
                StateHasChanged();
            }
        }
        else
        {
            selectedCell = celda;
            if (celda.Appointment is not null)
            {
                OpenManagementModal();
            }
            else
            {
                OpenAssignModal();
            }
        }
    }
    private void OpenReassignAssignModal()
    {
        tempAppointment = new();
        activeReassignMode = true;
        showPatientModal = true; // Primero selector de paciente
    }
    private void OpenAssignModal()
    {
        tempAppointment = new();
        showPatientModal = true; // Primero selector de paciente
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (shouldFocus)
        {
            shouldFocus = false; // Reinicia la bandera
            await patientReasonInputRef.FocusAsync();
        }
    }

    private void OpenManagementModal()
    {
        modalType = Modal_Type.Gestion;
        showModal = true;
    }

    private void ActivateMoveMode()
    {
        if (selectedCell?.Appointment is null) return;
        activeMoveMode = true;
        sourceCell = selectedCell;
        CloseModal();
    }

    private void CancelMoveMode()
    {
        activeMoveMode = false;
        sourceCell = null;
    }

    private void CloseModal()
    {
        _showModal = false;
        showModal = false;
        modalType = Modal_Type.Ninguno;
    }

    private void CloseOuterModal()
    {
        if (showModal)
        {
            CloseModal();
        }
    }

    // Guardar desde modal manual (paciente/motivo tipeado)
    private async Task SaveAppointmentFromModal()
    {
        if (selectedCell is null) return;
        if (string.IsNullOrWhiteSpace(tempAppointment.PatientName) || string.IsNullOrWhiteSpace(tempAppointment.Phone)) return;

        var appt = selectedCell.Appointment ?? new Appointment
        {
            //Id = Guid.NewGuid(),
            DateTime = selectedCell.DateTime,
            DoctorId = Guid.Parse("C771A686-20DB-4DE3-A547-60B368B1FA98"),
            IsDeleted = false
        };

        appt.PatientId = tempAppointment.PatientId;
        appt.PatientName = tempAppointment.PatientName;
        appt.Phone = tempAppointment.Phone;
        appt.Reason = tempAppointment.Reason;

        if (activeReassignMode)
        {
            showModal = false;
            activeReassignMode = false;
            await VM.ReassignAsync(appt);
        }
        else
        {
            await VM.SaveChange(appt);
        }

        var x = selectedCell.Appointment;
        CloseModal();
        StateHasChanged();
    }

    private async Task CancelAppointment()
    {
        if (selectedCell?.Appointment is null) return;

        // Marcado simple como cancelado en UI; persistencia mínima
        var appt = selectedCell.Appointment;
        appt.IsDeleted = true;
        selectedCell.Appointment = null;

        await VM.DeleteAsync(appt); // En un futuro: VM.CancelAppointment(appt.Id)
        CloseModal();
        StateHasChanged();
    }

    // Callback del selector de semanas
    private async Task OnWeekSelected((DateTime Start, DateTime End) week)
    {
        SelectedWeek = week;
        VM.DateBase = week.Start;
        await VM.LoadWeekAsync(week.Start, week.End);
        StateHasChanged();
    }

    // Callback del selector de pacientes
    private void OnPatientSelected((Guid idSelectedPatient, string nameSelectedPatient, string phoneSelectedPatient) selectedPatient)
    {
        showPatientModal = false;

        if (selectedCell is null) return;

        tempAppointment.PatientId = selectedPatient.idSelectedPatient;
        tempAppointment.PatientName = selectedPatient.nameSelectedPatient;
        tempAppointment.Phone = selectedPatient.phoneSelectedPatient;
        tempAppointment.Reason = ""; // Se mantiene compat: teléfono como motivo si no hay otro campo

        // detectar si es reasignar o nuevo.
        //selectedCell.Appointment = appt;
        shouldFocus = true; // Indica que debe enfocar al renderizar
        modalType = Modal_Type.Asignar;
        showModal = true;

        StateHasChanged();
    }

    // Utilidades
    //private List<string> GenerateDayIntervals(string inicioManana, string finManana, string inicioTarde, string finTarde)
    //{
    //    var resultado = new List<string>();

    //    var hIniM = TimeSpan.Parse(inicioManana);
    //    var hFinM = TimeSpan.Parse(finManana);
    //    var hIniT = TimeSpan.Parse(inicioTarde);
    //    var hFinT = TimeSpan.Parse(finTarde);

    //    var actual = hIniM;
    //    while (actual < hFinM)
    //    {
    //        resultado.Add(actual.ToString(@"hh\:mm"));
    //        actual = actual.Add(TimeSpan.FromMinutes(15));
    //    }

    //    actual = hIniT;
    //    while (actual < hFinT)
    //    {
    //        resultado.Add(actual.ToString(@"hh\:mm"));
    //        actual = actual.Add(TimeSpan.FromMinutes(15));
    //    }

    //    return resultado;
    //}

    private string Capitalize(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return texto;
        return CultureInfo.CurrentCulture.TextInfo.ToUpper(texto);
    }
}