namespace MedRec.Entity.Enums;
public enum ErrorCode
{
    None = 0,
    Cancelled,
    DuplicateKey,
    ConcurrencyError,
    ValidationError,
    DatabaseError,
    UpdateError,
    NotFound,
    Conflict,
    Unknown
}

