namespace MedRec.Shared.Exceptions;
public class PatientAlreadyExistsException : Exception
{
    public PatientAlreadyExistsException()
        : base("El paciente ya se encuentra registrado.") { }
}

