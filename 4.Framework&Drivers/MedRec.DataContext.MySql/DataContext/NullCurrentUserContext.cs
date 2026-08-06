using MedRec.Entity.Interfaces;

namespace MedRec.DataContext.MySql.DataContext;

internal class NullCurrentUserContext : ICurrentUserContext
{
    public Guid? UserId => null;
}
