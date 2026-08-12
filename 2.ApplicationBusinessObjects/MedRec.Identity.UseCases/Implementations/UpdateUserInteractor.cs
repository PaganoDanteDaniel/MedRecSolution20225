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

public class UpdateUserInteractor(
    IUpdateUserOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<UpdateUserDto> validatorHub) : IUpdateUserInputPort
{
    public async Task HandleAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_Edit, ct);

        var isValid = await validatorHub.Validate(dto, UpdateUserValidator.Validate);
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

        user.FullName = dto.FullName;
        user.ProfessionalId = dto.ProfessionalId;
        user.RowVersion = dto.RowVersion;

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await userCommandsRepository.UpdateAsync(user, dto.RoleIds, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
