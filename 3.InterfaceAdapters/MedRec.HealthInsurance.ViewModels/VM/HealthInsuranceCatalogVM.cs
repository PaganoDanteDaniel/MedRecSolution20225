using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.ViewModels.Models;
using MedRec.Shared.DTOs;

namespace MedRec.HealthInsurance.ViewModels.VM;
public class HealthInsuranceCatalogVM(
    IHealthInsuranceCatalogInputPort getInputPort,
    IHealthInsuranceCatalogOutputPort getOutputPort,
    IDeleteHealthInsuranceInputPort deleteInputPort,
    IDeleteHealthInsuranceOutputPort deleteOutputPort)
{


    private string _informationMessage;
    private int _totalRecords;


    #region Events
    public event Action OnCatalogLoaded;
    public event Action OnHealthInsuranceAdded;
    public event Action OnHealthInsuranceUpdated;
    public event Action OnHealthInsuranceDeleted;
    public event Action OnShowMessage;
    public event Action OnShowWarning;
    public event Action OnShowError;
    public event Action OnShowConcurrencyError;
    #endregion

    public IEnumerable<HealthInsuranceModel> HealthInsuranceCatalog { get; set; } = [];
    public string InformationMessage { get => _informationMessage; set => _informationMessage = value; }

    public int TotalRecords { get => _totalRecords; set => _totalRecords = value; }

    public async Task GetHealthInsuranceAsync(PaginationDto paginationDto, CancellationToken ct = default)
    {
        await getInputPort.Handle(paginationDto, ct);
        var result = getOutputPort.Result;

        if (result.HasValidationErrors)
        {

            InformationMessage = string.Join("<br />", result.ValidationErrors.Select(e => e.ErrorMessage));
            OnShowMessage?.Invoke();
        }
        else if (!result.Success)
        {
            HandleErrors(result.Error, result.MessageAction);
        }
        else
        {
            HealthInsuranceCatalog = result.Value.healthInsurancesCatalog.Select(dto => new HealthInsuranceModel()
            {
                Id = dto.Id,
                Name = dto.Name,
                Acronym = dto.Acronym
            }).ToList();

            TotalRecords = result.Value.totalRecords;

            OnCatalogLoaded?.Invoke();
        }

    }
    public async Task DeleteHealthInsuranceAsync(Guid healthCompanyId, CancellationToken ct = default)
    {
        try
        {
            await deleteInputPort.Handle(healthCompanyId, ct);
            var result = deleteOutputPort.Result;
            if (!result.Success)
            {
                HandleErrors(result.Error, result.MessageAction);
            }
            else
            {
                OnHealthInsuranceDeleted?.Invoke();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error crítico al eliminar la Obra Social", ex);
        }

    }
    private void HandleErrors(ErrorInfo error, UserMessageAction action)
    {
        InformationMessage = error?.Message ?? "Error desconocido.";

        switch (action)
        {
            case UserMessageAction.ShowWarning:
                OnShowWarning?.Invoke();
                break;
            case UserMessageAction.ShowConcurrencyMessage:
                InformationMessage += " VALORES EN LA BASE DE DATOS:<br />";
                if (error.Details is IReadOnlyList<ConcurrencyConflictDto> conflicts)
                {
                    InformationMessage += string.Join("<br />",
                        conflicts
                        .Where(x => x.PropertyName != "RowVersion")
                        .Select(x => $"{x.PropertyName}: {x.DataBaseValue}"));
                    InformationMessage += "<br /> VALORES QUE USTED ENVÍA:<br />";
                    InformationMessage += string.Join("<br />",
                        conflicts
                        .Where(x => x.PropertyName != "RowVersion")
                        .Select(x => $"{x.PropertyName}: {x.UserValue}"));
                }
                OnShowConcurrencyError?.Invoke();
                break;
            case UserMessageAction.ShowError:
                OnShowError?.Invoke();
                break;
            case UserMessageAction.ShowInfoMessage:
                OnShowMessage?.Invoke();
                break;
            default: // None o desconocido
                OnShowMessage?.Invoke();
                break;
        }
    }
}
