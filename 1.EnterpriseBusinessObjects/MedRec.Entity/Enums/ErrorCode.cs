namespace MedRec.Entity.Enums;
public enum ErrorCode
{
    None = 0,
    DuplicateKey,
    ConcurrencyError,
    ValidationError,
    DatabaseError,
    UpdateError,
    NotFound,
    Conflict,
    Unknown
}

