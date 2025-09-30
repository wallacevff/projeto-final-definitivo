using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Filters;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ActivityAudienceRepository(AppDbContext context)
    : DefaultRepository<ActivityAudience, Filter, Guid>(context), IActivityAudienceRepository
{
}
