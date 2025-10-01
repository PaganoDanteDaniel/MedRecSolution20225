namespace MedRec.Patients.BusinessObjects.Interfaces.Ports;
/// <summary>
/// Interface para obtener los detalles del paciente.
/// </summary>
public interface IPatientDetailsInputPort
{
    /// <summary>
    /// Maneja la obtención de detalles del paciente por ID.
    /// </summary>
    /// <param name="patientId">El ID del paciente.</param>
    /// <returns>Una tarea que representa la operación asincrónica.</returns>
    Task Handle(Guid patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Maneja la obtención de detalles del paciente por número de documento.
    /// </summary>
    /// <param name="documentNumber">El número de documento del paciente.</param>
    /// <returns>Una tarea que representa la operación asincrónica.</returns>
    Task Handle(string documentNumber, CancellationToken cancellationToken = default);
}
