using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.ViewModels.Models;

namespace MedRec.MedicalAppointments.ViewModels.VM;

public class WeeklyScheduleViewModel
{
    // Puente con casos de uso (InputPorts) y Presenters (OutputPorts)
    private readonly ICreateMedicalAppointmentInputPort _createIn;
    private readonly ICreateMedicalAppointmentOutputPort _createOut;
    private readonly IDeleteMedicalAppointmentInputPort _deleteIn;
    private readonly IDeleteMedicalAppointmentOutputPort _deleteOut;
    private readonly IGetMedicalAppointmentsInputPort _getIn;
    private readonly IGetMedicalAppointmentsOutputPort _getOut;
    private readonly IMoveMedicalAppointmentInputPort _moveIn;
    private readonly IMoveMedicalAppointmentOutputPort _moveOut;
    private readonly IReassignMedicalAppointmentInputPort _reassignIn;
    private readonly IReassignMedicalAppointmentOutputPort _reassignOut;

    // Estado local para la grilla/semanario
    private readonly List<Appointment> _appointments = new();

    public WeeklyScheduleViewModel(
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
        _createIn = createIn;
        _createOut = createOut;
        _deleteIn = deleteIn;
        _deleteOut = deleteOut;
        _getIn = getIn;
        _getOut = getOut;
        _moveIn = moveIn;
        _moveOut = moveOut;
        _reassignIn = reassignIn;
        _reassignOut = reassignOut;
    }

    // Consulta semanal (modelo UI -> usa input port; DTOs sólo para interacción con capa de aplicación)
    public async Task<List<Appointment>> GetAppointments(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        await _getIn.Handle((startDate, endDate), ct);

        // Mapea resultado del presenter (DTO) al modelo de UI
        var dtos = _getOut.AppointmentsDto;
        _appointments.Clear();
        _appointments.AddRange(dtos.Select(ToModel));

        return _appointments.ToList();
    }

    // Crear turno (entrada/salida en modelo de UI; se mapea al DTO correspondiente internamente)
    public async Task SaveChange(Appointment appointment, CancellationToken ct = default)
    {
        var createDto = new CreateMedicalAppointmentDto(
            appointment.DateTime,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.Reason ?? string.Empty);

        await _createIn.Handle(createDto, ct);

        var createdDto = _createOut.AppointmentDto;
        UpsertLocal(ToModel(createdDto));
    }

    // Mover turno: usa el modelo y sólo mapea al DTO requerido para el input port
    // Requiere la nueva fecha y la RowVersion actual para control de concurrencia.
    public async Task MoveAsync(Appointment appointment, CancellationToken ct = default)
    {
        var moveDto = new MoveMedicalAppointmentDto(
            appointment.Id,
            appointment.DateTime,
            appointment.RowVersion ?? Array.Empty<byte>());

        await _moveIn.Handle(moveDto, ct);

        var movedDto = _moveOut.movedMedicalAppointmentDto;
        UpsertLocal(ToModel(movedDto));
    }

    // Reasignar médico: usa el modelo y genera el DTO que requiere el input port
    public async Task ReassignAsync(Appointment appointment, CancellationToken ct = default)
    {
        var reassignDto = new MedicalAppointmentDto(
            appointment.Id,
            appointment.DateTime,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.Reason ?? string.Empty,
            appointment.RowVersion ?? Array.Empty<byte>(),
            appointment.IsDeleted);

        await _reassignIn.Handle(reassignDto, ct);

        var reassignedDto = _reassignOut.ReassignedAppointmentDto;
        UpsertLocal(ToModel(reassignedDto));
    }

    // Eliminar turno (sólo ID del modelo)
    public async Task<bool> DeleteAsync(Appointment appointment, CancellationToken ct = default)
    {
        await _deleteIn.Handle(appointment.Id, ct);
        var deleted = _deleteOut.IsDeleted;

        if (deleted)
        {
            var idx = _appointments.FindIndex(a => a.Id == appointment.Id);
            if (idx >= 0) _appointments.RemoveAt(idx);
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
            Reason = dto.Reason,
            RowVersion = dto.RowVersion,
            IsDeleted = dto.IsDeleted,
            PatientFirstName = dto.PatientFirstName,
            PatientLastName = dto.PatientLastName,
            Phone = dto.PatientPhoneNumber
        };

    private void UpsertLocal(Appointment appt)
    {
        var idx = _appointments.FindIndex(a => a.Id == appt.Id);
        if (idx >= 0) _appointments[idx] = appt;
        else _appointments.Add(appt);
    }
}

// Nota: Se asume que el tipo Appointment (modelo de UI) contiene como mínimo:
// Guid Id, AppointmentDateTime AppointmentDateTime, Guid PatientId, Guid DoctorId, string? Reason, byte[]? RowVersion.
// Ajusta el mapeo si tu modelo difiere.
