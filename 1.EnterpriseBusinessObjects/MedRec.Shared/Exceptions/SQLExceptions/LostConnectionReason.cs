namespace MedRec.Shared.Exceptions.SQLExceptions;

public enum LostConnectionReason
{
    UnableToConnect,
    ServerGoneAway,
    ConnectionLostDuringQuery,
    TooManyConnections,
    StatementInterrupted,
    Timeout,
    Unknown
}

