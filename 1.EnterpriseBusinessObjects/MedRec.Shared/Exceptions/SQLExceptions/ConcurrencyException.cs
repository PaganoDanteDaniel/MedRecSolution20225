using MedRec.Shared.DTOs;

namespace MedRec.Shared.Exceptions.SQLExceptions;

public class ConcurrencyException : UpdateException
{
    public ConcurrencyException(string message, Exception inner = null, object details = null)
        : base(message, inner, details: details) { }

    // Acceso tipado a la lista de conflictos
    public IReadOnlyList<ConcurrencyConflictDto>? Conflicts
        => Details as IReadOnlyList<ConcurrencyConflictDto>;
}
