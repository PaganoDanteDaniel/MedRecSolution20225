using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Patients.UseCases.Implementations;
/// <summary>
/// Interactor para manejar la eliminación de un paciente.
/// </summary>
/// 
/// <param name="_outputPort">El presentador para notificar los resultados.</param>
/// <param name="_commandRepository">La unidad de trabajo para manejar la eliminación del paciente.</param>
internal class DeletePatientInteractor : IDeletePatientInputPort
{
    private readonly IDeletePatientOutputPort _presenter;
    private readonly IPatientCommandsRepository _commandRepository;
    private readonly IPatientQueriesRepository _queriesRepository;
    private readonly IRepositoryUnitOfWork _unitOfWork;

    public DeletePatientInteractor(
        IDeletePatientOutputPort presenter,
        IPatientCommandsRepository commandRepository,
        IPatientQueriesRepository queriesRepository,
        IRepositoryUnitOfWork unitOfWork)
    {
        _presenter = presenter;
        _commandRepository = commandRepository;
        _queriesRepository = queriesRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Maneja la lógica para eliminar un paciente.
    /// </summary>
    /// <param name="id">El ID del paciente a eliminar.</param>
    public async Task Handle(Guid id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (id == Guid.Empty)
        {
            await _presenter.ErrorAsync(new ErrorInfo(
                "No ha proporcionado el identificador para la eliminación del paciente.",
                ErrorCode.ValidationError,
                httpStatusCode: 400));
            return;
        }

        await _unitOfWork.ExecuteWithRetry(async () =>
        {
            var entity = await _queriesRepository.GetPatientById(id, ct);
            if (entity is null)
            {
                await _presenter.ErrorAsync(new ErrorInfo(
                    $"El paciente indicado, no existe o ya fue eliminado.",
                    ErrorCode.NotFound,
                    new { PatientId = id },
                    404));
                return;
            }

            entity.IsDeleted = true;
            await _commandRepository.SoftDelete(entity, ct);
            await _unitOfWork.SaveChanges(ct);
            await _presenter.ErrorAsync(null);
        }, ct);
        await _presenter.Handle(ct);
    }
}
