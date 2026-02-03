using MedRec.Shared.Exceptions.SQLExceptions;
using MySqlConnector;
using System.Data.Common;
using System.Net.Sockets;

namespace MedRec.DataContext.MySql.UnitOfWork;

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
                if (msg.Contains("UNABLE TO CONNECT") || msg.Contains("HOST") && msg.Contains("CONNECT"))
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

            if (inner is IOException ioEx)
            {
                // Caso: SocketException interna (más preciso)
                if (ioEx.InnerException is SocketException socketEx)
                {
                    // Códigos comunes de socket en Windows:
                    // - 10053: WSAECONNABORTED → "An established connection was aborted by the software in your host machine"
                    // - 10054: WSAECONNRESET → "Connection reset by peer"
                    // - 10060: WSAETIMEDOUT  → "Connection timed out"
                    switch (socketEx.ErrorCode)
                    {
                        case 10053: // ConnectionAborted
                        case 10054: // ConnectionReset
                        case 10060: // Timeout
                        case 10061: // ConnectionRefused (aunque esto suele ser al conectar)
                            reason = LostConnectionReason.ConnectionLostDuringQuery;
                            return true;
                    }
                }

                // Caso: detección por mensaje (para compatibilidad multiplataforma o .NET en Linux/macOS)
                var ioMsg = ioEx.Message;
                if (ioMsg.Contains("anulado una conexión establecida", StringComparison.OrdinalIgnoreCase) ||      // es-ES
                    ioMsg.Contains("established connection was aborted", StringComparison.OrdinalIgnoreCase) ||  // en-US
                    ioMsg.Contains("connection was forcibly closed", StringComparison.OrdinalIgnoreCase) ||
                    ioMsg.Contains("connection reset", StringComparison.OrdinalIgnoreCase) ||
                    ioMsg.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                {
                    reason = LostConnectionReason.ConnectionLostDuringQuery;
                    return true;
                }
            }

            // 4. TimeoutException (por si se lanza directamente)
            if (inner is TimeoutException)
            {
                reason = LostConnectionReason.Timeout;
                return true;
            }
        }

        return false;
    }
}