using MedRec.Entity.DTOs;

namespace MedRec.Shared.Exceptions;
public class RepositoryException : PersistenceException
{
    public ErrorInfo Error { get; }

    public RepositoryException(ErrorInfo error, Exception innerException = null)
        : base(error?.Message ?? "Repository error", innerException)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }
}
