namespace MedRec.Entity.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
}
