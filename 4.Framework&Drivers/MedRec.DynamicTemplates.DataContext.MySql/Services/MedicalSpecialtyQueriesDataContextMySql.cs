using MedRec.DataContext.MySql.DataContext;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;

namespace MedRec.DynamicTemplates.DataContext.MySql.Services;

internal class MedicalSpecialtyQueriesDataContextMySql(MedRecContext context) :
    IMedicalSpecialtyQueriesDataContext
{
    public async Task<IEnumerable<MedicalSpecialty>> GetActiveSpecialtiesAsync(CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await context.MedicalSpecialties
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cts);
    }

    public async Task<MedicalSpecialty?> GetByIdAsync(Guid id, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await context.MedicalSpecialties
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cts);
    }

    public async Task<MedicalSpecialty?> GetByNameAsync(string name, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await context.MedicalSpecialties
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == name, cts);
    }
}