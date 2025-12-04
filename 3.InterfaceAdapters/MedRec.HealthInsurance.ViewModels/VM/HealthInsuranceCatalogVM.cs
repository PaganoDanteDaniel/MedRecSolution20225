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


        if (getOutputPort.ValidationErrors?.Any() == true)
        {

            InformationMessage = string.Join("<br />", getOutputPort.ValidationErrors.Select(e => e.ErrorMessage));
            OnShowMessage?.Invoke();
        }
        else if (getOutputPort.ErrorMessage is not null)
        {
            InformationMessage = getOutputPort.ErrorMessage.Message;
            OnShowMessage?.Invoke();
        }
        else
        {
            HealthInsuranceCatalog = getOutputPort.HealthInsuranceCatalog.Select(dto => new HealthInsuranceModel()
            {
                Id = dto.Id,
                Name = dto.Name,
                Acronym = dto.Acronym
            }).ToList();

            TotalRecords = getOutputPort.TotalRecords;

            OnCatalogLoaded?.Invoke();
        }

    }
    public async Task DeleteHealthInsuranceAsync(Guid healthCompanyId, CancellationToken ct = default)
    {
        try
        {
            await deleteInputPort.Handle(healthCompanyId, ct);

            if (deleteOutputPort.ErrorMessage is not null)
            {
                HandleErrors(deleteOutputPort.ErrorMessage);
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
    private void HandleErrors(ErrorInfo error)
    {
        InformationMessage = error.Message;

        switch (error.Code)
        {
            case ErrorCode.DuplicateKey:
                OnShowWarning?.Invoke();
                break;
            case ErrorCode.ConcurrencyError:
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
            case ErrorCode.DatabaseError:
                OnShowError?.Invoke();
                break;
            default:
                OnShowMessage?.Invoke();
                break;
        }
    }
}
