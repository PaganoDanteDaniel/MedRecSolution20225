
using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Guard;
using MedRec.DataContext.MySql.Options;
using MedRec.Entity.POCOEntities;
using MedRec.MedicalVisit.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MedRec.MedicalVisit.DataContext.MySql.Services;
internal class MedicalVisitCommandDataContextMySql(IOptions<DBOptionsMySql> options) :
   DataBaseContextMySql(options), IMedicalVisitCommandDataContext
{

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        await Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
        await Database.CommitTransactionAsync(cancellationToken);
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
        await Database.RollbackTransactionAsync(cancellationToken);
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await GuardDBContext.AgainstSaveChangesErrorAsync(base.SaveChangesAsync, cancellationToken);
    public async Task CreateAsync(PatientMedicalVisit medicalVisit, CancellationToken cts = default) =>
        await PatientMedicalVisits.AddAsync(medicalVisit, cts);

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await operation();
        });
    }

    public async Task CreateMedicalHistoryAsync(PatientMedicalHistory medHist, CancellationToken cts = default) =>
        await PatientMedicalHistories.AddAsync(medHist, cts);



}
