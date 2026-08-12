using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.BusinessObjects.Validators;
using MedRec.Entity.Interfaces;
using MedRec.Validator.Interfaces;

namespace MedRec.Identity.UseCases.Implementations;

public class CreateUserInteractor(
    ICreateUserOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IPasswordHasher passwordHasher,
    IEmailNotificationService emailNotificationService,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<CreateUserDto> validatorHub) : ICreateUserInputPort
{
    public async Task HandleAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_Create, ct);

        var isValid = await validatorHub.Validate(dto, CreateUserValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var existing = await userQueriesRepository.GetByEmailAsync(dto.Email, ct);
        if (existing is not null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Ya existe un usuario con ese email.", ErrorCode.DuplicateKey, null, 409));
            return;
        }

        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = passwordHasher.Hash(dto.TemporaryPassword),
            IsActive = true,
            MustChangePassword = true,
            ProfessionalId = dto.ProfessionalId
        };

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await userCommandsRepository.CreateAsync(user, dto.RoleIds, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await emailNotificationService.SendTemporaryPasswordAsync(dto.Email, dto.FullName, dto.TemporaryPassword, ct);

        await presenter.Handle(ct);
    }
}
