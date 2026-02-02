using MedRec.Entity.Enums;
using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.ViewModels.Models;
using System.Globalization;

namespace MedRec.MedicalAppointments.ViewModels.VM;

public class WeeklyScheduleViewModel(
        ICreateMedicalAppointmentInputPort createIn,
        ICreateMedicalAppointmentOutputPort createOut,
        IDeleteMedicalAppointmentInputPort deleteIn,
        IDeleteMedicalAppointmentOutputPort deleteOut,
        IGetMedicalAppointmentsInputPort getIn,
        IGetMedicalAppointmentsOutputPort getOut,
        IMoveMedicalAppointmentInputPort moveIn,
        IMoveMedicalAppointmentOutputPort moveOut,
        IReassignMedicalAppointmentInputPort reassignIn,
        IReassignMedicalAppointmentOutputPort reassignOut)
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

    public DateTime DateBase { get => _dateBase; set => _dateBase = value; }

    public async Task LoadWeekAsync(DateTime start, DateTime end)
    {
        _lastLoadedWeek = (start, end);
        var citas = await GetAppointments(start, end);
        //var citas = new List<Appointment> { };
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
        await getIn.Handle((startDate, endDate), ct);

        // Mapea resultado del presenter (DTO) al modelo de UI
        var dtos = getOut.AppointmentsDto;
        _appointments.Clear();
        _appointments.AddRange(dtos.Select(ToModel));

        return _appointments.ToList();
    }

    // Crear turno (entrada/salida en modelo de UI; se mapea al DTO correspondiente internamente)
    public async Task SaveChange(Appointment appointment, CancellationToken ct = default)
    {
        try
        {
            var createDto = new CreateMedicalAppointmentDto(
                appointment.DateTime,
                appointment.PatientId,
                appointment.DoctorId,
                appointment.Reason.ToUpperInvariant() ?? string.Empty);

            await createIn.Handle(createDto, ct);
            if (createOut.ErrorMessage is not null)
            {
                var error = createOut.ErrorMessage;
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
            else if (createOut.AppointmentDto is not null)
            {
                var createdDto = createOut.AppointmentDto;

                // Actualiza la misma instancia que usa la UI
                appointment.DateTime = createdDto.DateTime;
                appointment.RowVersion = createdDto.RowVersion;
                appointment.PatientId = createdDto.PatientId;
                appointment.DoctorId = createdDto.DoctorId;
                appointment.Reason = createdDto.Reason.ToUpperInvariant();
                appointment.IsDeleted = createdDto.IsDeleted;

                await UpsertLocal(ToModel(createdDto));
                OnFinnishOperation?.Invoke();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al crear el turno", ex);
        }
    }

    // Mover turno: usa el modelo y sólo mapea al DTO requerido para el input port
    // Requiere la nueva fecha y la RowVersion actual para control de concurrencia.
    public async Task MoveAsync(Appointment appointment, CancellationToken ct = default)
    {
        try
        {
            var moveDto = new MoveMedicalAppointmentDto(
                appointment.Id,
                appointment.DateTime,
                appointment.RowVersion ?? Array.Empty<byte>());

            await moveIn.Handle(moveDto, ct);

            if (moveOut.ErrorMessage is not null)
            {
                var error = moveOut.ErrorMessage;
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
            else if (moveOut.movedMedicalAppointmentDto is not null)
            {
                var movedDto = moveOut.movedMedicalAppointmentDto;

                // Actualiza la misma instancia que usa la UI
                appointment.DateTime = movedDto.DateTime;
                appointment.RowVersion = movedDto.RowVersion;
                appointment.PatientId = movedDto.PatientId;
                appointment.DoctorId = movedDto.DoctorId;
                appointment.Reason = movedDto.Reason.ToUpperInvariant();
                appointment.IsDeleted = movedDto.IsDeleted;

                await UpsertLocal(ToModel(movedDto));
                OnFinnishOperation?.Invoke();
            }
        }
        catch (Exception ex)
        {
            // Para ErrorBoundary
            throw new InvalidOperationException("Error crítico al mover el turno", ex);
        }
    }
    // Reasignar médico: usa el modelo y genera el DTO que requiere el input port
    public async Task ReassignAsync(Appointment appointment, CancellationToken ct = default)
    {
        try
        {
            var reassignDto = new MedicalAppointmentDto(
            appointment.Id,
            appointment.DateTime,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.Reason.ToUpperInvariant() ?? string.Empty,
            appointment.RowVersion ?? Array.Empty<byte>(),
            appointment.IsDeleted);

            await reassignIn.Handle(reassignDto, ct);
            if (reassignOut.ErrorMessage is not null)
            {
                var error = reassignOut.ErrorMessage;
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
            else
            {
                var reassignedDto = reassignOut.ReassignedAppointmentDto;

                // Actualiza la misma instancia que usa la UI
                appointment.DateTime = reassignedDto.DateTime;
                appointment.RowVersion = reassignedDto.RowVersion;
                appointment.PatientId = reassignedDto.PatientId;
                appointment.DoctorId = reassignedDto.DoctorId;
                appointment.Reason = reassignedDto.Reason.ToUpperInvariant();
                appointment.IsDeleted = reassignedDto.IsDeleted;

                await UpsertLocal(ToModel(reassignedDto));
                OnFinnishOperation?.Invoke();
            }

        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al reasignar el turno", ex);
        }

    }
    // Eliminar turno (sólo ID del modelo)
    public async Task<bool> DeleteAsync(Appointment appointment, CancellationToken ct = default)
    {
        bool deleted = false;
        try
        {
            await deleteIn.Handle(appointment.Id, ct);

            if (deleteOut.ErrorMessage is not null)
            {
                var error = deleteOut.ErrorMessage;
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
            else
            {
                deleted = deleteOut.IsDeleted;

                if (deleted)
                {
                    var idx = _appointments.FindIndex(a => a.Id == appointment.Id);
                    if (idx >= 0) _appointments.RemoveAt(idx);
                }
                OnFinnishOperation?.Invoke();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al eliminar el turno", ex);
        }
        return deleted;
    }
    // Mapeos entre DTOs de aplicación y modelo de UI
    private static Appointment ToModel(MedicalAppointmentDto dto) =>
        new Appointment
        {
            Id = dto.Id,
            DateTime = dto.DateTime,
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Reason = dto.Reason.ToUpperInvariant(),
            RowVersion = dto.RowVersion,
            IsDeleted = dto.IsDeleted,
            PatientFirstName = dto.PatientFirstName,
            PatientLastName = dto.PatientLastName,
            Phone = dto.PatientPhoneNumber
        };
    private async Task UpsertLocal(Appointment appt)
    {


        var idx = _appointments.FindIndex(a => a.Id == appt.Id);
        if (idx >= 0)
        {
            var target = _appointments[idx];
            target.DateTime = appt.DateTime;
            target.PatientId = appt.PatientId;
            target.DoctorId = appt.DoctorId;
            target.Reason = appt.Reason.ToUpperInvariant();
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
    private List<string> GenerateDayIntervals(string inicioManana, string finManana, string inicioTarde, string finTarde)
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

}
