using MedRec.Shared.Exceptions.SQLExceptions;
using MySqlConnector;
using System.Data.Common;

namespace MedRec.DataContext.MySql.Infrastructure;

// Implementación específica para MySQL
internal sealed class MySqlExceptionClassifier : IDbConnectionExceptionClassifier
{
    public bool TryClassify(Exception ex, out LostConnectionReason reason, out int? providerErrorCode)
    {
        reason = LostConnectionReason.Unknown;
        providerErrorCode = null;

        for (var inner = ex; inner is not null; inner = inner.InnerException!)
        {
            if (inner is MySqlException mySqlEx)
            {
                providerErrorCode = mySqlEx.Number;
                reason = mySqlEx.Number switch
                {
                    1042 or 2002 or 2003 => LostConnectionReason.UnableToConnect,
                    2006 => LostConnectionReason.ServerGoneAway,
                    2013 => LostConnectionReason.ConnectionLostDuringQuery,
                    1040 => LostConnectionReason.TooManyConnections,
                    3571 => LostConnectionReason.StatementInterrupted,
                    1205 => LostConnectionReason.Timeout,
                    _ => LostConnectionReason.Unknown
                };
                return true;
            }

            if (inner is DbException dbEx)
            {
                var msg = (dbEx.Message ?? string.Empty).ToUpperInvariant();
                if (msg.Contains("UNABLE TO CONNECT") || (msg.Contains("HOST") && msg.Contains("CONNECT")))
                {
                    reason = LostConnectionReason.UnableToConnect; return true;
                }
                if (msg.Contains("SERVER HAS GONE AWAY"))
                {
                    reason = LostConnectionReason.ServerGoneAway; return true;
                }
                if (msg.Contains("LOST CONNECTION"))
                {
                    reason = LostConnectionReason.ConnectionLostDuringQuery; return true;
                }
                if (msg.Contains("TOO MANY CONNECTIONS"))
                {
                    reason = LostConnectionReason.TooManyConnections; return true;
                }
                if (msg.Contains("TIMEOUT"))
                {
                    reason = LostConnectionReason.Timeout; return true;
                }
            }
        }

        return false;
    }
}