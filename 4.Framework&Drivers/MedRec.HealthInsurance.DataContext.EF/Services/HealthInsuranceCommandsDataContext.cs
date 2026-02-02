
using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace MedRec.HealthInsurance.DataContext.EF.Services;
internal class HealthInsuranceCommandsDataContext(MedRecContext context) :
    IHealthInsuranceCommandsDataContext
{
    public async Task CreateAsync(HealthInsuranceCompany healthCompany, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.HealthInsuranceCompanies.AddAsync(healthCompany);
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error al crear el registro en la base de datos.",
                              ErrorCode.UpdateError,
                              ex.InnerException?.Message ?? ex.Message, 500));
        }
        catch (Exception ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error inesperado en la capa de persistencia.",
                              ErrorCode.Unknown,
                              ex.Message, 500));
        }

    }
    public async Task UpdateAsync(HealthInsuranceCompany entity, CancellationToken ct = default)
    {
        try
        {
            // Paso 1: Asegurar que no haya una entidad rastreada con el mismo Id
            var trackedEntry = context.ChangeTracker.Entries<HealthInsuranceCompany>()
                .FirstOrDefault(e => e.Entity.Id == entity.Id);

            if (trackedEntry != null)
            {
                trackedEntry.State = EntityState.Detached;
            }
            var existing = await context.HealthInsuranceCompanies
                .FirstOrDefaultAsync(h => h.Id == entity.Id, ct);
            if (existing is null)
                throw new BusinessException(new ErrorInfo("Obra Social no encontrada.", ErrorCode.NotFound, entity.Id, 404));

            context.Entry(existing).CurrentValues.SetValues(entity);
            context.Entry(existing).Property("RowVersion").OriginalValue = existing.RowVersion;

        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error al actualizar el registro en la base de datos.",
                              ErrorCode.UpdateError,
                              ex.InnerException?.Message ?? ex.Message, 500));
        }
        catch (Exception ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error inesperado en la capa de persistencia.",
                              ErrorCode.Unknown,
                              ex.Message, 500));
        }

    }
    public async Task DeleteAsync(HealthInsuranceCompany healthCompany, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await context.HealthInsuranceCompanies.FindAsync(healthCompany.Id, cancellationToken);
            if (entity != null) { context.HealthInsuranceCompanies.Remove(entity); }
        }
        catch (DbUpdateException ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error al eliminar el registro en la base de datos.",
                              ErrorCode.UpdateError,
                              ex.InnerException?.Message ?? ex.Message, 500));
        }
        catch (Exception ex)
        {
            throw new BusinessException(
                new ErrorInfo("Error inesperado en la capa de persistencia.",
                              ErrorCode.Unknown,
                              ex.Message, 500));
        }

    }
}