using MedRec.Entity.Enums;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration;

namespace MedRec.Professionals.ViewModels.VM;
public class ProfessionalsListVM(
    IListProfessionalsInputPort listInteractor,
    IListProfessionalsOutputPort listPresenter,
    IDeleteProfessionalInputPort deleteInteractor,
    IDeleteProfessionalOutputPort deletePresenter)
{
    public IReadOnlyList<ProfessionalModel> Professionals { get; private set; } = Array.Empty<ProfessionalModel>();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;

    public async Task LoadAsync(ProfessionalType? typeFilter = null, CancellationToken ct = default)
    {
        IsProcessing = true;
        InformationMessage = string.Empty;
        try
        {
            await listInteractor.HandleAsync(typeFilter, ct);
            var result = listPresenter.Result;
            Professionals = result.Success
                ? (result.Value ?? Array.Empty<MedRec.Professionals.BusinessObjects.DTOs.ProfessionalDto>()).Select(ProfessionalMapper.ToModel).ToArray()
                : Array.Empty<ProfessionalModel>();
            if (!result.Success)
                InformationMessage = result.Error?.Message ?? "No se pudo cargar el listado de profesionales.";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await deleteInteractor.HandleAsync(id, ct);
            var result = deletePresenter.Result;
            if (!result.Success)
                InformationMessage = result.Error?.Message ?? "No se pudo eliminar el profesional.";
            else
                await LoadAsync(ct: ct);
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
