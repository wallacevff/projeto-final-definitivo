using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IActivityAudienceRepository : IDefaultRepository<ActivityAudience, Filter, Guid>
{
}
