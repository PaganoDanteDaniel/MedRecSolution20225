using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions;

public class CreateUserForProfessionalAction(
    ICreateUserInputPort inPort,
    ICreateUserOutputPort outPort) : ICreateUserForProfessionalAction
{
    public async Task<OperationResult<bool>> ExecuteAsync(
        Guid professionalId,
        string email,
        string fullName,
        string temporaryPassword,
        IReadOnlyList<Guid> roleIds,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var dto = new CreateUserDto(email, fullName, temporaryPassword, roleIds, professionalId);
            await inPort.HandleAsync(dto, ct);
            return outPort.Result;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<bool>(new ErrorInfo($"Error crítico al crear el usuario: {ex.Message}"), null);
        }
    }
}
