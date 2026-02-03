using MedRec.DataContext.MySql.DataContext;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;

namespace MedRec.DynamicTemplates.DataContext.MySql.Services;

internal class MedicalVisitDynamicFieldQueriesDataContextMySql(MedRecContext context) :
    IMedicalVisitDynamicFieldQueriesDataContext
{
    public async Task<IEnumerable<MedicalVisitDynamicField>> GetByVisitIdAsync(Guid visitId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await context.MedicalVisitDynamicFields
            .AsNoTracking()
            .Where(f => f.PatientMedicalVisitId == visitId)
            .ToListAsync(cts);
    }

    public async Task<MedicalVisitDynamicField?> GetByVisitAndFieldDefinitionAsync(Guid visitId, Guid fieldDefinitionId, CancellationToken cts = default)
    {
        cts.ThrowIfCancellationRequested();

        return await context.MedicalVisitDynamicFields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.PatientMedicalVisitId == visitId && f.FieldDefinitionId == fieldDefinitionId, cts);
    }
}