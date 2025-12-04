using MedRec.Entity.Enums;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.ViewModels.Models;
using MedRec.Shared.DTOs;
using MedRec.Shared.Enums;
using System.Collections.ObjectModel;

namespace MedRec.Patients.ViewModels.VM;

public class UpdatePatientVM(
    IUpdatePatientInputPort interactor,
    IUpdatePatientOutputPort presenter,
    IPatientDetailsInputPort detailsInteractor,
    IPatientDetailsOutputPort detailsPresenter)
{
    private readonly IUpdatePatientInputPort _interactor = interactor;
    private readonly IUpdatePatientOutputPort _presenter = presenter;
    private readonly IPatientDetailsInputPort _detailsInteractor = detailsInteractor;
    private readonly IPatientDetailsOutputPort _detailsPresenter = detailsPresenter;

    public ObservableCollection<ConflictFieldModel> ConcurrencyConflicts { get; } = [];

    public event Action? OnPatientUpdated;
    public event Action? OnShowMessage;
    public event Action? OnShowWarning;
    public event Action? OnShowError;
    public event Action? OnShowConcurrencyError;

    public UpdatePatientModel Model { get; set; } = new();
    public string InformationMessage { get; set; } = string.Empty;

    public async Task GetPatient(Guid patientId, CancellationToken cts = default)
    {
        try
        {
            InformationMessage = string.Empty;
            ConcurrencyConflicts.Clear();

            await _detailsInteractor.Handle(patientId, cts);
            var result = _detailsPresenter.Result;

            if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "Error desconocido.";

                switch (result.MessageAction)
                {
                    case UserMessageAction.ShowWarning:
                        OnShowWarning?.Invoke();
                        break;
                    case UserMessageAction.ShowConcurrencyMessage:
                        OnShowConcurrencyError?.Invoke();
                        break;
                    case UserMessageAction.ShowError:
                        OnShowError?.Invoke();
                        break;
                    case UserMessageAction.ShowInfoMessage:
                        OnShowMessage?.Invoke();
                        break;
                }
                return;
            }

            MapToModel(result.Value!);
        }
        catch (Exception ex)
        {
            // Propagar para ErrorBoundary con contexto
            throw new InvalidOperationException("Error crítico al obtener paciente.", ex);
        }
    }

    public async Task UpdatePatient(CancellationToken cts = default)
    {
        try
        {
            InformationMessage = string.Empty;

            // El interactor/usos del repositorio deben adjuntar un stub, marcar Modified
            // y establecer UserValue del token de concurrencia (RowVersion) según Microsoft Docs.
            await _interactor.Handle((UpdatePatientDto)Model, cts);

            var result = _presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join("<br />", result.ValidationErrors.Select(x => x.ErrorMessage));
                OnShowMessage?.Invoke();
                return;
            }

            if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "Error desconocido.";

                switch (result.MessageAction)
                {
                    case UserMessageAction.ShowWarning:
                        OnShowWarning?.Invoke();
                        break;

                    case UserMessageAction.ShowConcurrencyMessage:
                        // Limpiar lista previa para evitar duplicados en reintentos
                        ConcurrencyConflicts.Clear();

                        if (result.Error?.Details is IReadOnlyList<ConcurrencyConflictDto> conflictList)
                        {
                            foreach (var dto in conflictList)
                            {
                                // Si el conflicto incluye la nueva RowVersion desde BD, actualizar el modelo
                                if (dto.PropertyName == nameof(Model.RowVersion) && dto.DataBaseValue is byte[] rv)
                                {
                                    Model.RowVersion = rv;
                                }
                                else
                                {
                                    ConcurrencyConflicts.Add(new ConflictFieldModel(dto));
                                }
                            }
                        }
                        OnShowConcurrencyError?.Invoke();
                        break;

                    case UserMessageAction.ShowError:
                        OnShowError?.Invoke();
                        break;

                    case UserMessageAction.ShowInfoMessage:
                        OnShowMessage?.Invoke();
                        break;
                }
                return;
            }

            // Éxito: limpiar modelo para evitar reenvíos accidentales
            Model = new UpdatePatientModel();
            ConcurrencyConflicts.Clear();
            OnPatientUpdated?.Invoke();
        }
        catch (Exception)
        {
            InformationMessage = "Error inesperado al actualizar el paciente.";
            OnShowError?.Invoke();
        }
    }

    private void MapToModel(PatientDetailDto dto)
    {
        Model.Id = dto.Id;
        Model.FirstName = dto.FirstName;
        Model.LastName = dto.LastName;
        Model.DocumentNumber = dto.DocumentNumber;
        Model.Address = dto.Address;
        Model.PhoneNumber = dto.PhoneNumber;
        Model.Email = dto.Email;
        Model.DateOfBirth = dto.DateOfBirth;
        Model.BiologicalSexId = dto.BiologicalSexId;
        Model.HealthInsuranceCompanyId = dto.HealthInsuranceCompanyId;
        Model.SelectedHealthCompanyName = dto.HealthInsuranceCompanyName;
        Model.HealthInsuranceMemberNumber = dto.HealthInsuranceMemberNumber;
        Model.HealthInsuranceCard = dto.HealthInsuranceCard;
        Model.HealthInsurancePlan = dto.HealthInsurancePlan;
        Model.RowVersion = dto.RowVersion;
    }

    public async Task RetryWithResolvedValues(CancellationToken ct = default)
    {
        // Aplicar resoluciones al modelo (KeepDbValue/KeepUserValue)
        foreach (var conflict in ConcurrencyConflicts)
        {
            var prop = conflict.Label;
            var value = conflict.Resolution switch
            {
                ResolutionChoice.KeepDbValue => conflict.DbValue,
                ResolutionChoice.KeepUserValue => conflict.UserValue,
                ResolutionChoice.EditManually => throw new NotSupportedException("EditManually no implementado aún"),
                _ => conflict.UserValue
            };

            switch (prop)
            {
                case nameof(Model.FirstName): Model.FirstName = value?.ToString() ?? string.Empty; break;
                case nameof(Model.LastName): Model.LastName = value?.ToString() ?? string.Empty; break;
                case nameof(Model.DocumentNumber): Model.DocumentNumber = value?.ToString() ?? string.Empty; break;
                case nameof(Model.Address): Model.Address = value?.ToString() ?? string.Empty; break;
                case nameof(Model.PhoneNumber): Model.PhoneNumber = value?.ToString() ?? string.Empty; break;
                case nameof(Model.Email): Model.Email = value?.ToString() ?? string.Empty; break;
                case nameof(Model.DateOfBirth): Model.DateOfBirth = value is DateTime dt ? dt : default; break;
                case nameof(Model.BiologicalSexId):
                    if (value is BiologicalSex sex)
                        Model.BiologicalSexId = sex;
                    else if (value is int sexInt)
                        Model.BiologicalSexId = (BiologicalSex)sexInt;
                    else
                        Model.BiologicalSexId = default;
                    break;
                case nameof(Model.HealthInsuranceCompanyId):
                    Model.HealthInsuranceCompanyId = value switch
                    {
                        Guid g => g,
                        string s when Guid.TryParse(s, out var g) => g,
                        null => null,
                        _ => null
                    };
                    break;
                case nameof(Model.HealthInsuranceMemberNumber): Model.HealthInsuranceMemberNumber = value?.ToString() ?? string.Empty; break;
                case nameof(Model.HealthInsuranceCard): Model.HealthInsuranceCard = value?.ToString() ?? string.Empty; break;
                case nameof(Model.HealthInsurancePlan): Model.HealthInsurancePlan = value?.ToString() ?? string.Empty; break;
            }
        }

        // Reintentar con el RowVersion actualizado (si fue provisto por BD)
        await UpdatePatient(ct);
    }
}