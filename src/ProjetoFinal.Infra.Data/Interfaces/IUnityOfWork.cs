using Microsoft.EntityFrameworkCore.Storage;

namespace Talonario.Domain.Repositories;

public interface IUnityOfWork
{
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    public IDbContextTransaction BeginTransaction();
    public void CommitTransaction();
    public int SaveChanges();
}