using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration;

namespace MedRec.Professionals.ViewModels.VM;
public class UpdateProfessionalVM(
    IUpdateProfessionalInputPort updateInteractor,
    IUpdateProfessionalOutputPort updatePresenter,
    IGetProfessionalByIdInputPort getInteractor,
    IGetProfessionalByIdOutputPort getPresenter)
{
    public UpdateProfessionalModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;
    public bool Success { get; private set; }

    public async Task LoadAsync(Guid id, CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await getInteractor.HandleAsync(id, ct);
            var result = getPresenter.Result;
            if (result.Success && result.Value is not null)
            {
                var m = ProfessionalMapper.ToModel(result.Value);
                Model = new UpdateProfessionalModel
                {
                    Id = m.Id,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Phone = m.Phone,
                    Type = m.Type,
                    LicenseNumber = m.LicenseNumber,
                    SpecialtyId = m.SpecialtyId,
                    RowVersion = result.Value.RowVersion
                };
            }
            else
            {
                InformationMessage = result.Error?.Message ?? "Profesional no encontrado.";
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public async Task UpdateAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        Success = false;
        try
        {
            InformationMessage = string.Empty;
            await updateInteractor.HandleAsync(ProfessionalMapper.ToUpdateDto(Model), ct);
            var result = updatePresenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo editar el profesional.";
            }
            else
            {
                InformationMessage = "Profesional actualizado correctamente.";
                Success = true;
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
