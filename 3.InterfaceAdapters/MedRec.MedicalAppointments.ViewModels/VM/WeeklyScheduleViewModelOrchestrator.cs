using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.MedicalAppointments.ViewModels.Models;
using MedRec.MedicalAppointments.ViewModels.Orchestration;
using System.Globalization;

namespace MedRec.MedicalAppointments.ViewModels.VM;

public class WeeklyScheduleViewModelOrchestrator(IAppointmentOrchestrator orchestrator)
{
    // Estado local para la grilla/semanario
    private readonly List<Appointment> _appointments = new();
    private readonly List<ScheduleRow> _rows = new();
    private DateTime _dateBase = DateTime.Today;
    private (DateTime Start, DateTime End)? _lastLoadedWeek;

    public string InformationMessage { get; set; }
    public event Action OnFinnishOperation;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;

    public List<ScheduleRow> Rows => _rows;

    public List<DateTime> WeekDays
    {
        get
        {
            // Ajustar para que Sunday se trate como día 7 (no 0)
            var dayOfWeek = (int)DateBase.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7; // Sunday = 7

            var lunes = DateBase.AddDays(1 - dayOfWeek); // ahora sí, lunes de la misma semana

            return Enumerable.Range(0, 5)
                             .Select(i => lunes.Date.AddDays(i))
                             .ToList();
        }
    }

    public List<string> TimeSlots => GenerateDayIntervals("09:30", "12:30", "17:30", "20:30");

    public DateTime DateBase
    {
        get => _dateBase;
        set => _dateBase = value;
    }

    public async Task LoadWeekAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        _lastLoadedWeek = (start, end);
        var citas = await GetAppointments(start, end, ct);

        var mapa = citas?
            .Where(a => a is not null)
            .ToDictionary(a => a.DateTime.ToString("yyyy-MM-dd HH:mm")) ?? new Dictionary<string, Appointment>();

        Rows.Clear();

        foreach (var hora in TimeSlots)
        {
            var row = new ScheduleRow { Time = hora };

            foreach (var dia in WeekDays)
            {
                var dt = DateTime.ParseExact($"{dia:yyyy-MM-dd} {hora}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                var celda = new ScheduleCell { DateTime = dt };
                if (mapa.TryGetValue($"{dt:yyyy-MM-dd HH:mm}", out var appt) && !appt.IsDeleted)
                    celda.Appointment = appt;

                row.Cells.Add(celda);
            }

            Rows.Add(row);
        }
    }

    public async Task<List<Appointment>> GetAppointments(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var result = await orchestrator.GetByRangeAsync(startDate, endDate, ct);

        if (!result.Success)
        {
            RaiseFromError(result.Error);
            return _appointments.ToList();
        }

        _appointments.Clear();
        _appointments.AddRange(result.Value ?? Array.Empty<Appointment>());
        return _appointments.ToList();
    }

    // Crear turno (entrada/salida en modelo de UI)
    public async Task SaveChange(Appointment appointment, CancellationToken ct = default)
    {
        try
        {
            var result = await orchestrator.CreateAsync(appointment, ct);

            if (!result.Success)
            {
                if (result.ValidationErrors?.Any() == true)
                {
                    InformationMessage = string.Join("<br />", result.ValidationErrors.Select(e => e.ErrorMessage));
                    OnShowMessage?.Invoke();
                }
                else if (result.Error is not null)
                {
                    RaiseFromError(result.Error);
                }
                return;
            }

            if (result.Value is not null)
            {
                var created = result.Value;

                // Actualiza la misma instancia que usa la UI
                appointment.Id = created.Id;
                appointment.DateTime = created.DateTime;
                appointment.RowVersion = created.RowVersion;
                appointment.PatientId = created.PatientId;
                appointment.DoctorId = created.DoctorId;
                appointment.Reason = (created.Reason ?? string.Empty).ToUpperInvariant();
                appointment.IsDeleted = created.IsDeleted;
                appointment.PatientFirstName = created.PatientFirstName;
                appointment.PatientLastName = created.PatientLastName;
                appointment.Phone = created.Phone;

                await UpsertLocal(created);
                OnFinnishOperation?.Invoke();

            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al crear el turno", ex);
        }
    }

    // Mover turno: requiere nueva fecha y RowVersion actual para concurrencia
    public async Task<bool> MoveAsync(Appointment appointment, CancellationToken ct = default)
    {
        var success = false;
        try
        {
            var result = await orchestrator.MoveAsync(appointment, ct);

            if (!result.Success)
            {
                if (result.ValidationErrors?.Any() == true)
                {
                    InformationMessage = string.Join("<br />", result.ValidationErrors.Select(e => e.ErrorMessage));
                    OnShowMessage?.Invoke();
                }
                else if (result.Error is not null)
                {
                    RaiseFromError(result.Error);
                }
                return false;
            }

            if (result.Value is not null)
            {
                var moved = result.Value;

                // Actualiza la misma instancia que usa la UI
                appointment.DateTime = moved.DateTime;
                appointment.RowVersion = moved.RowVersion;
                appointment.PatientId = moved.PatientId;
                appointment.DoctorId = moved.DoctorId;
                appointment.Reason = (moved.Reason ?? string.Empty).ToUpperInvariant();
                appointment.IsDeleted = moved.IsDeleted;
                appointment.PatientFirstName = moved.PatientFirstName;
                appointment.PatientLastName = moved.PatientLastName;
                appointment.Phone = moved.Phone;

                await UpsertLocal(moved);
                OnFinnishOperation?.Invoke();
                success = true;
            }
        }
        catch (Exception ex)
        {
            // No relanzar: notificar UI y devolver false
            InformationMessage = $"Error crítico al mover el turno: {ex.Message}";
            OnShowError?.Invoke();
            return false;
        }
        return success;
    }

    // Reasignar médico
    public async Task<bool> ReassignAsync(Appointment appointment, CancellationToken ct = default)
    {
        var success = false;
        try
        {
            var result = await orchestrator.ReassignAsync(appointment, ct);

            if (!result.Success)
            {
                if (result.ValidationErrors?.Any() == true)
                {
                    InformationMessage = string.Join("<br />", result.ValidationErrors.Select(e => e.ErrorMessage));
                    OnShowMessage?.Invoke();
                }
                else if (result.Error is not null)
                {
                    RaiseFromError(result.Error);
                }
                return false;
            }

            if (result.Value is not null)
            {
                var reassigned = result.Value;

                // Actualiza la misma instancia que usa la UI
                appointment.DateTime = reassigned.DateTime;
                appointment.RowVersion = reassigned.RowVersion;
                appointment.PatientId = reassigned.PatientId;
                appointment.DoctorId = reassigned.DoctorId;
                appointment.Reason = (reassigned.Reason ?? string.Empty).ToUpperInvariant();
                appointment.IsDeleted = reassigned.IsDeleted;
                appointment.PatientFirstName = reassigned.PatientFirstName;
                appointment.PatientLastName = reassigned.PatientLastName;
                appointment.Phone = reassigned.Phone;

                await UpsertLocal(reassigned);
                OnFinnishOperation?.Invoke();
                success = true;
            }
        }
        catch (Exception ex)
        {
            InformationMessage = $"Error crítico al reasignar el turno: {ex.Message}";
            OnShowError?.Invoke();
            return false;
        }
        return success;
    }

    // Eliminar turno
    public async Task<bool> DeleteAsync(Appointment appointment, CancellationToken ct = default)
    {
        bool deleted = false;
        try
        {
            var result = await orchestrator.DeleteAsync(appointment.Id, ct);

            if (!result.Success)
            {
                RaiseFromError(result.Error);
                return false;
            }

            deleted = result.Value;

            if (deleted)
            {
                var idx = _appointments.FindIndex(a => a.Id == appointment.Id);
                if (idx >= 0) _appointments.RemoveAt(idx);

                OnFinnishOperation?.Invoke();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al eliminar el turno", ex);
        }
        return deleted;
    }

    private async Task UpsertLocal(Appointment appt)
    {
        var idx = _appointments.FindIndex(a => a.Id == appt.Id);
        if (idx >= 0)
        {
            var target = _appointments[idx];
            target.DateTime = appt.DateTime;
            target.PatientId = appt.PatientId;
            target.DoctorId = appt.DoctorId;
            target.Reason = (appt.Reason ?? string.Empty).ToUpperInvariant();
            target.RowVersion = appt.RowVersion;
            target.IsDeleted = appt.IsDeleted;
            target.PatientFirstName = appt.PatientFirstName;
            target.PatientLastName = appt.PatientLastName;
            target.Phone = appt.Phone;

            if (_lastLoadedWeek.HasValue)
            {
                await LoadWeekAsync(_lastLoadedWeek.Value.Start, _lastLoadedWeek.Value.End);
            }
        }
        else
        {
            _appointments.Add(appt);

            if (_lastLoadedWeek.HasValue)
            {
                await LoadWeekAsync(_lastLoadedWeek.Value.Start, _lastLoadedWeek.Value.End);
            }
        }
    }

    private static List<string> GenerateDayIntervals(string inicioManana, string finManana, string inicioTarde, string finTarde)
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

    private void RaiseFromError(ErrorInfo? error)
    {
        if (error is null)
        {
            InformationMessage = "Error desconocido.";
            OnShowMessage?.Invoke();
            return;
        }

        InformationMessage = error.Message;

        switch (error.Code)
        {
            case ErrorCode.DuplicateKey:
                OnShowWarning?.Invoke();
                break;
            case ErrorCode.ConcurrencyError:
                OnShowConcurrencyError?.Invoke();
                break;
            case ErrorCode.DatabaseError:
                OnShowError?.Invoke();
                break;
            default:
                OnShowMessage?.Invoke();
                break;
        }
    }
}