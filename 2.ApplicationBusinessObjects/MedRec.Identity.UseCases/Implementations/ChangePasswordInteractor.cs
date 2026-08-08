using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Identity.UseCases.Implementations;

public class ChangePasswordInteractor(
    IChangePasswordOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IPasswordHasher passwordHasher,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<ChangePasswordDto> validatorHub) : IChangePasswordInputPort
{
    public async Task HandleAsync(ChangePasswordDto dto, CancellationToken ct = default)
    {
        var isValid = await validatorHub.Validate(dto, ChangePasswordValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var userId = currentUserContext.UserId!.Value;
        var user = await userQueriesRepository.GetByIdAsync(userId, ct);
        if (user is null || !passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            await presenter.InvalidCurrentPassword();
            return;
        }

        var newHash = passwordHasher.Hash(dto.NewPassword);

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await userCommandsRepository.SetPasswordAsync(userId, newHash, false, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
