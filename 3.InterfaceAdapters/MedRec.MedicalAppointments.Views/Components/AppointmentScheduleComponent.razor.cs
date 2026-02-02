using MedRec.CommonComponents.Views;
using MedRec.MedicalAppointments.ViewModels.Models;
using MedRec.MedicalAppointments.ViewModels.VM;
using MedRec.Patients.Views.Components;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace MedRec.MedicalAppointments.Views.Components;
public partial class AppointmentScheduleComponent
{
    private WeeklyScheduleViewModelOrchestrator VM => Service;

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

    #region === Callbacks para el nuevo modal ===

    private EventCallback _onOkCallback = default;
    private EventCallback _onCancelCallback = default;
    private EventCallback _onDeleteCallback = default;
    private EventCallback _onRetryCallback = default;

    #endregion

    private ElementReference patientReasonInputRef;
    private bool shouldFocus = false; // Bandera para controlar el foco

    private string shift = "";
    private bool activeMoveMode = false;
    private bool activeReassignMode = false;
    private bool isLoading = false;

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


    private void SetLoading(bool loading)
    {
        isLoading = loading;
        InvokeAsync(StateHasChanged);
    }

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

        VM.OnFinnishOperation += () => ShowModalMessage("Actualización exitosa...", ModalType.MessageSuccess);
        VM.OnReloadData += ReloadData;

        StateHasChanged();
        try
        {
            SetLoading(true);
            await VM.LoadWeekAsync(start, end); // o la llamada concreta
        }
        finally
        {
            SetLoading(false);
        }
    }
    private async Task ReloadData()
    {
        if (SelectedWeek is not null)
        {
            VM.DateBase = SelectedWeek.Value.Start;
            SetLoading(true);
            await VM.LoadWeekAsync(SelectedWeek.Value.Start, SelectedWeek.Value.End);
            SetLoading(false);
            StateHasChanged();
        }

    }
    private void ShowModalMessage(string title, ModalType type)
    {
        ShowOk = true;
        CloseOnOverlayClick = true;
        ModalTitle = title;
        ModalType = type;
        ModalBody = null;
        ModalMessage = VM.InformationMessage;
        _onOkCallback = EventCallback.Factory.Create(this, CloseModal);
        _showModal = true;
        _navigateAfterClose = false;
        InvokeAsync(StateHasChanged);
    }
    // CSS por estado de celda
    private string GetCssClass(ScheduleCell celda)
    {
        if (celda.IsPast) return "pasado";
        if (activeMoveMode && sourceCell is not null && celda.Key != sourceCell.Key && celda.Appointment is null) return "modo-mover";
        if (celda.Appointment is not null) return "asignado";
        return "";
    }
    private async Task CeldaClic(ScheduleCell celda)
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
                // Intento de mover: no actualizamos visualmente hasta confirmar éxito
                var appt = sourceCell.Appointment;
                var originalDateTime = appt.DateTime;

                // Preparamos el modelo con la nueva fecha/hora
                appt.DateTime = celda.DateTime;
                SetLoading(true);
                var movedOk = await VM.MoveAsync(appt);
                SetLoading(false);
                if (movedOk)
                {
                    // Actualizamos UI solo si se confirmó el movimiento
                    sourceCell.Appointment = null;
                    celda.Appointment = appt;
                }
                else
                {
                    // Revertimos el modelo si falló la operación
                    appt.DateTime = originalDateTime;
                }

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

        CloseModal();
        activeReassignMode = true;

        _showModal = true; // Primero selector de paciente
    }
    private void OpenAssignModal()
    {
        tempAppointment = new();
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
        CloseActionModal();
    }
    private void CancelMoveMode()
    {
        activeMoveMode = false;
        sourceCell = null;
    }
    // Cierra ambos modales (mensaje + gestión)
    private void CloseModal()
    {
        _showModal = false;
        showModal = false;
        modalType = Modal_Type.Ninguno;
    }

    // Cierra solo el modal de gestión (no toca el modal de mensajes)
    private void CloseActionModal()
    {
        showModal = false;
        modalType = Modal_Type.Ninguno;
    }

    private void CloseOuterModal()
    {
        if (showModal)
        {
            CloseActionModal();
        }
    }

    // Guardar desde modal manual (paciente/motivo tipeado)
    private async Task SaveAppointmentFromModal()
    {
        if (selectedCell is null) return;

        if (string.IsNullOrWhiteSpace(tempAppointment.PatientName) || string.IsNullOrWhiteSpace(tempAppointment.Phone)) return;

        Appointment? originalAppt = new();

        var appt = selectedCell.Appointment ?? new Appointment
        {
            //Id = Guid.NewGuid(),
            DateTime = selectedCell.DateTime,
            DoctorId = Guid.Parse("C771A686-20DB-4DE3-A547-60B368B1FA98"),
            IsDeleted = false
        };

        if (appt.Id != Guid.Empty)
        {
            originalAppt = new Appointment()
            {
                Id = appt.Id,
                DateTime = appt.DateTime,
                PatientId = appt.PatientId,
                DoctorId = appt.DoctorId,
                PatientLastName = appt.PatientLastName,
                PatientFirstName = appt.PatientFirstName,
                PatientName = appt.PatientName,
                Phone = appt.Phone,
                Reason = appt.Reason,
                IsDeleted = appt.IsDeleted,
                RowVersion = appt.RowVersion
            };
        }

        appt.PatientId = tempAppointment.PatientId;
        appt.PatientName = tempAppointment.PatientName;
        appt.Phone = tempAppointment.Phone;
        appt.Reason = tempAppointment.Reason;

        if (activeReassignMode)
        {
            CloseActionModal();
            activeReassignMode = false;
            SetLoading(true);
            if (!await VM.ReassignAsync(appt) && originalAppt != null)
            {
                appt.Id = originalAppt.Id;
                appt.DateTime = originalAppt.DateTime;
                appt.PatientId = originalAppt.PatientId;
                appt.DoctorId = originalAppt.DoctorId;
                appt.PatientLastName = originalAppt.PatientLastName;
                appt.PatientFirstName = originalAppt.PatientFirstName;
                appt.PatientName = originalAppt.PatientName;
                appt.Phone = originalAppt.Phone;
                appt.Reason = originalAppt.Reason;
                appt.IsDeleted = originalAppt.IsDeleted;
                appt.RowVersion = originalAppt.RowVersion;
                SetLoading(false);
            }
            SetLoading(false);
        }
        else
        {
            CloseActionModal();
            SetLoading(true);
            await VM.SaveChange(appt);
            SetLoading(false);
        }

        // Cerrar solo el modal de gestión para no interferir con el de mensaje

        StateHasChanged();
    }

    private async Task CancelAppointment()
    {
        if (selectedCell?.Appointment is null) return;

        var appt = selectedCell.Appointment;
        var originalIsDeleted = appt.IsDeleted;

        SetLoading(true);
        var deleted = await VM.DeleteAsync(appt);
        SetLoading(false);
        if (deleted)
        {
            selectedCell.Appointment = null;
            // Mostrar éxito ya lo maneja el VM con OnFinnishOperation
            CloseActionModal();
        }
        else
        {
            appt.IsDeleted = originalIsDeleted;
            // No cerrar el modal de mensaje. Solo cerrar el de gestión.
            CloseActionModal();
        }

        StateHasChanged();
    }

    private async Task OnWeekSelected((DateTime Start, DateTime End) week)
    {
        SelectedWeek = week;
        VM.DateBase = SelectedWeek.Value.Start;
        SetLoading(true);
        await VM.LoadWeekAsync(SelectedWeek.Value.Start, SelectedWeek.Value.End);
        SetLoading(false);
        StateHasChanged();
    }

    // Callback del selector de pacientes
    private void OnPatientSelected((Guid idSelectedPatient, string nameSelectedPatient, string phoneSelectedPatient) selectedPatient)
    {
        _showModal = false;

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
    private string Capitalize(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return texto;
        return CultureInfo.CurrentCulture.TextInfo.ToUpper(texto);
    }
}