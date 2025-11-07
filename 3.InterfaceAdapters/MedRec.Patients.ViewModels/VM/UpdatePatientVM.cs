using MedRec.Entity.Enums;
using MedRec.Patients.BusinessObjects.DTOs;
using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.ViewModels.Models;

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

    public event Action OnPatientUpdated;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;

    public UpdatePatientModel Model { get; set; } = new();
    public string InformationMessage { get; set; }
    public async Task GetPatient(Guid patientId, CancellationToken cts = default)
    {
        try
        {
            InformationMessage = "";
            await _detailsInteractor.Handle(patientId, cts);
            if (_detailsPresenter.ErrorMessage is not null)
            {
                var error = _detailsPresenter.ErrorMessage;
                InformationMessage = error.Message;

                switch (error.Code)
                {
                    case ErrorCode.DuplicateKey:
                        OnShowWarning?.Invoke();
                        break;
                    case ErrorCode.ConcurrencyError:
                        OnShowConcurrencyError?.Invoke();
                        break;
                    case ErrorCode.DatabaseError:
                        OnShowError?.Invoke();
                        break;
                    default:
                        OnShowMessage?.Invoke();
                        break;
                }
            }
            else if (_detailsPresenter.PatientDetails is not null)
            {
                var p = _detailsPresenter.PatientDetails;

                Model.Id = p.Id;
                Model.FirstName = p.FirstName;
                Model.LastName = p.LastName;
                Model.DocumentNumber = p.DocumentNumber;
                Model.Address = p.Address;
                Model.PhoneNumber = p.PhoneNumber;
                Model.Email = p.Email;
                Model.DateOfBirth = p.DateOfBirth;
                Model.BiologicalSexId = p.BiologicalSexId;
                Model.HealthInsuranceCompanyId = p.HealthInsuranceCompanyId;
                Model.SelectedHealthCompanyName = p.HealthInsuranceCompanyName;
                Model.HealthInsuranceMemberNumber = p.HealthInsuranceMemberNumber;
                Model.HealthInsuranceCard = p.HealthInsuranceCard;
                Model.HealthInsurancePlan = p.HealthInsurancePlan;
                Model.RowVersion = p.RowVersion;
            }
        }
        catch (Exception ex)
        {
            // Para ErrorBoundary
            throw new InvalidOperationException("Error crítico al obtener el paciente", ex);
        }
    }
    public async Task UpdatePatient(CancellationToken cts = default)
    {
        try
        {
            InformationMessage = "";
            await _interactor.Handle((UpdatePatientDto)Model, cts);

            if (_presenter.ValidationErrors?.Any() == true)
            {
                InformationMessage = string.Join("<br />", _presenter.ValidationErrors.Select(
                    x => x.ErrorMessage).ToArray());
                OnShowMessage.Invoke();
            }
            else if (_presenter.ErrorMessage is not null)
            {
                var error = _presenter.ErrorMessage;
                InformationMessage = error.Message;

                switch (error.Code)
                {
                    case ErrorCode.DuplicateKey:
                        OnShowWarning?.Invoke();
                        break;
                    case ErrorCode.ConcurrencyError:
                        OnShowConcurrencyError?.Invoke();
                        break;
                    case ErrorCode.DatabaseError:
                        OnShowError?.Invoke();
                        break;
                    default:
                        OnShowMessage?.Invoke();
                        break;
                }
            }
            else if (_presenter.UpdatedSuccessfully)
            {
                Model = new UpdatePatientModel();
                OnPatientUpdated?.Invoke();
            }
        }
        catch (Exception ex)
        {
            // Para ErrorBoundary
            throw new InvalidOperationException("Error crítico al actualizar el paciente", ex);
        }
    }
}
