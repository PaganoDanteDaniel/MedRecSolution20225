using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Shared.ErrorHandling;
using MedRec.Shared.Exceptions;
using MedRec.Shared.Exceptions.SQLExceptions;

public sealed class DefaultExceptionToErrorInfoMapper : IExceptionToErrorInfoMapper
{
    public ErrorInfo Map(Exception ex) => ex switch
    {
        // Cancelación explícita (499)
        OperationCanceledException => new ErrorInfo(
            "Operación cancelada durante la ejecución.",
            ErrorCode.Cancelled,
            null,
            499),

        // Conexión perdida (transitorio - 503)
        LostConnectionException lce => new ErrorInfo(
            lce.Message,
            ErrorCode.DatabaseError,
            new { lce.Reason, lce.MySqlErrorNumber },
            503),

        // Validaciones de parámetros (400)
        ArgumentOutOfRangeException aoor => new ErrorInfo(
            aoor.Message,
            ErrorCode.ValidationError,
            new { aoor.ParamName, aoor.ActualValue },
            400),

        ArgumentException ae => new ErrorInfo(
            ae.Message,
            ErrorCode.ValidationError,
            new { ae.ParamName },
            400),

        // Concurrencia (409)
        ConcurrencyException cx => new ErrorInfo(
            cx.Message,
            ErrorCode.ConcurrencyError,
            cx.Conflicts ?? cx.Details,
            409),

        // Clave duplicada (409)
        DuplicateKeyException dx => new ErrorInfo(
            dx.Message,
            ErrorCode.DuplicateKey,
            dx.Entities,
            409),

        // Error de actualización genérico (500)
        UpdateException ux => new ErrorInfo(
            ux.Message,
            ErrorCode.UpdateError,
            new { ux.Entities, ux.Details },
            500),

        // BusinessException trae su propio ErrorInfo (passthrough)
        BusinessException bx => bx.Error,

        // RepositoryException trae su propio ErrorInfo (passthrough)
        RepositoryException rx => rx.Error,

        // Persistencia genérica (500)
        PersistenceException px => new ErrorInfo(
            px.Message,
            ErrorCode.DatabaseError,
            new { px.Message },
            500),

        // InvalidOperationException sigue siendo fallo interno (500)
        InvalidOperationException ioe => new ErrorInfo(
            ioe.Message,
            ErrorCode.DatabaseError,
            new { ioe.Message },
            500),

        // Fallback para cualquier otra excepción no controlada (500)
        _ => new ErrorInfo(
            "Error inesperado al procesar la solicitud.",
            ErrorCode.DatabaseError,
            new { ex.Message, ex.StackTrace },
            500)
    };
}
