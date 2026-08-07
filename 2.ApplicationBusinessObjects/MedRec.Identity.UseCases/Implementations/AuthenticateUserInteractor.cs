using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Identity.UseCases.Implementations;

public class AuthenticateUserInteractor(
    IAuthenticateUserOutputPort presenter,
    IUserQueriesRepository userQueriesRepository,
    IPasswordHasher passwordHasher,
    IAuthTokenGenerator tokenGenerator,
    IModelValidatorHub<AuthenticateUserDto> validatorHub) : IAuthenticateUserInputPort
{
    public async Task HandleAsync(AuthenticateUserDto dto, CancellationToken ct = default)
    {
        var isValid = await validatorHub.Validate(dto, AuthenticateUserValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var user = await userQueriesRepository.GetByEmailAsync(dto.Email, ct);
        if (user is null || !user.IsActive || !passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            await presenter.InvalidCredentials();
            return;
        }

        var roles = await userQueriesRepository.GetRoleNamesAsync(user.Id, ct);
        var permissions = await userQueriesRepository.GetPermissionCodesAsync(user.Id, ct);
        var (token, expiresAtUtc) = tokenGenerator.GenerateToken(user.Id, user.Email, roles, permissions);

        await presenter.Handle(
            new AuthResultDto(user.Id, user.Email, user.FullName, user.DoctorId, roles, permissions, token, expiresAtUtc, user.MustChangePassword),
            ct);
    }
}
