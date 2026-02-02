namespace MedRec.Shared.Exceptions.SQLExceptions;
public interface IDbConnectionExceptionClassifier
{
    /// <summary>
    /// Intenta clasificar la excepción como pérdida de conexión (transitoria o no).
    /// </summary>
    /// <param name="ex">Excepción capturada.</param>
    /// <param name="reason">Motivo clasificado.</param>
    /// <param name="providerErrorCode">Código numérico del proveedor (si aplica).</param>
    /// <returns>true si se pudo clasificar; false en caso contrario.</returns>
    bool TryClassify(Exception ex, out LostConnectionReason reason, out int? providerErrorCode);
}
