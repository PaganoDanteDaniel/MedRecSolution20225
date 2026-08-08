using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Identity.UseCases.Implementations;

public class ResetUserPasswordInteractor(
    IResetUserPasswordOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IPasswordHasher passwordHasher,
    IEmailNotificationService emailNotificationService,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<ResetUserPasswordDto> validatorHub) : IResetUserPasswordInputPort
{
    public async Task HandleAsync(ResetUserPasswordDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_Edit, ct);

        var isValid = await validatorHub.Validate(dto, ResetUserPasswordValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var user = await userQueriesRepository.GetByIdAsync(dto.UserId, ct);
        if (user is null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Usuario no encontrado.", ErrorCode.NotFound, null, 404));
            return;
        }

        var hash = passwordHasher.Hash(dto.TemporaryPassword);

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await userCommandsRepository.SetPasswordAsync(dto.UserId, hash, true, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await emailNotificationService.SendTemporaryPasswordAsync(user.Email, user.FullName, dto.TemporaryPassword, ct);

        await presenter.Handle(ct);
    }
}
