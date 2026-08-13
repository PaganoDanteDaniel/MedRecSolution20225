using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class CreateUserPresenter : BaseOutputPort<bool>, ICreateUserOutputPort
{
    public Task Handle(bool emailSent, CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(
            emailSent,
            emailSent ? null : "El usuario se creó correctamente, pero no se pudo enviar el email con la contraseña temporal. Comunicásela manualmente.");
        return Task.CompletedTask;
    }
}
