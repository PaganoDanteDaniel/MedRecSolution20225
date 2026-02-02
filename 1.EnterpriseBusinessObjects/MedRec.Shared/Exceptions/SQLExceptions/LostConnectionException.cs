namespace MedRec.Shared.Exceptions.SQLExceptions;
public sealed class LostConnectionException : Exception
{
    public LostConnectionReason Reason { get; }
    public int? MySqlErrorNumber { get; }
    public bool IsTransient { get; }

    public LostConnectionException(
        string message,
        LostConnectionReason reason,
        int? mySqlErrorNumber = null,
        bool isTransient = true,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Reason = reason;
        MySqlErrorNumber = mySqlErrorNumber;
        IsTransient = isTransient;
    }

    public override string ToString() =>
        $"{base.ToString()} | Reason={Reason}, MySqlErrorNumber={MySqlErrorNumber}, Transient={IsTransient}";
}

