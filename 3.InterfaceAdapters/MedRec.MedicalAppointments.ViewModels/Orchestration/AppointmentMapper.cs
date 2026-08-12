using MedRec.MedicalAppointments.BusinessObjects.DTOs;
using MedRec.MedicalAppointments.ViewModels.Models;

namespace MedRec.MedicalAppointments.ViewModels.Orchestration;

internal static class AppointmentMapper
{
    public static Appointment ToModel(MedicalAppointmentDto dto) => new()
    {
        Id = dto.Id,
        DateTime = dto.DateTime,
        PatientId = dto.PatientId,
        ProfessionalId = dto.ProfessionalId,
        Reason = (dto.Reason ?? string.Empty).ToUpperInvariant(),
        RowVersion = dto.RowVersion,
        IsDeleted = dto.IsDeleted,
        PatientFirstName = dto.PatientFirstName,
        PatientLastName = dto.PatientLastName,
        Phone = dto.PatientPhoneNumber
    };

    public static CreateMedicalAppointmentDto ToCreateDto(Appointment model) =>
        new(model.DateTime, model.PatientId, model.ProfessionalId, (model.Reason ?? string.Empty).ToUpperInvariant());

    public static MoveMedicalAppointmentDto ToMoveDto(Appointment model) =>
        new(model.Id, model.DateTime, model.RowVersion ?? Array.Empty<byte>());

    public static MedicalAppointmentDto ToReassignDto(Appointment model) =>
        new(model.Id,
            model.DateTime,
            model.PatientId,
            model.ProfessionalId,
            (model.Reason ?? string.Empty).ToUpperInvariant(),
            model.RowVersion ?? Array.Empty<byte>(),
            model.IsDeleted);
}