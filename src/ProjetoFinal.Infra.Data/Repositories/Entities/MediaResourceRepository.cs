using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class MediaResourceRepository(AppDbContext context)
    : DefaultRepository<MediaResource, MediaResourceFilter, Guid>(context), IMediaResourceRepository
{
    public Task<MediaResource?> GetByShaAsync(string sha256, CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(media => media.Sha256 == sha256, cancellationToken);
    }
}
