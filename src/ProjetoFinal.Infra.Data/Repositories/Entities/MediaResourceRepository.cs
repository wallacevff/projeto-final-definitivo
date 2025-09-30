using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class MediaResourceRepository(AppDbContext context)
    : DefaultRepository<MediaResource, MediaResourceFilter, Guid>(context), IMediaResourceRepository
{
}
