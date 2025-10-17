using Microsoft.EntityFrameworkCore.Storage;
using ProjetoFinal.Infra.Data.Contexts;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Infra.Data.UnityOfWorks;

public class UnityOfWork(AppDbContext appDbContext) : IUnityOfWork
{
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return appDbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return appDbContext.Database.CommitTransactionAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return appDbContext.SaveChangesAsync(cancellationToken);
    }

    public IDbContextTransaction BeginTransaction()
    {
        return appDbContext.Database.BeginTransaction();
    }

    public void CommitTransaction()
    {
        appDbContext.Database.CommitTransaction();
    }

    public int SaveChanges()
    {
        return appDbContext.SaveChanges();
    }
}