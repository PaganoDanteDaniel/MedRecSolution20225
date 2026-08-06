using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Interfaces.Services;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class CurrentUserContext(ISessionService sessionService) : ICurrentUserContext
{
    public Guid? UserId => sessionService.CurrentUser?.UserId;
}
