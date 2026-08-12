using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Professionals.UseCases.Implementations;

public class CreateProfessionalInteractor(
    ICreateProfessionalOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<CreateProfessionalDto> validatorHub) : ICreateProfessionalInputPort
{
    public async Task HandleAsync(CreateProfessionalDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_Create, ct);

        var isValid = await validatorHub.Validate(dto, CreateProfessionalValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var existing = await professionalRepository.GetByEmailAsync(dto.Email, ct);
        if (existing is not null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Ya existe un profesional con ese email.", ErrorCode.DuplicateKey, null, 409));
            return;
        }

        var professional = new Professional
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone ?? string.Empty,
            HireDate = dto.HireDate,
            Type = dto.Type,
            LicenseNumber = dto.LicenseNumber,
            SpecialtyId = dto.SpecialtyId,
            IsDeleted = false
        };

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await professionalRepository.CreateAsync(professional, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(professional.Id, ct);
    }
}
