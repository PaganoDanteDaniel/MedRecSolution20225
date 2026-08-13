using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Professionals.UseCases.Implementations;

public class UpdateProfessionalInteractor(
    IUpdateProfessionalOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<UpdateProfessionalDto> validatorHub) : IUpdateProfessionalInputPort
{
    public async Task HandleAsync(UpdateProfessionalDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_Edit, ct);

        var isValid = await validatorHub.Validate(dto, UpdateProfessionalValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var professional = await professionalRepository.GetByIdAsync(dto.Id, ct);
        if (professional is null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Profesional no encontrado.", ErrorCode.NotFound, null, 404));
            return;
        }

        professional.FirstName = dto.FirstName;
        professional.LastName = dto.LastName;
        professional.Phone = dto.Phone;
        professional.Type = dto.Type;
        professional.LicenseNumber = dto.Type is ProfessionalType.Doctor or ProfessionalType.Nurse ? dto.LicenseNumber : null;
        professional.SpecialtyId = dto.Type == ProfessionalType.Doctor ? dto.SpecialtyId : null;
        professional.RowVersion = dto.RowVersion;

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await professionalRepository.UpdateAsync(professional, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
