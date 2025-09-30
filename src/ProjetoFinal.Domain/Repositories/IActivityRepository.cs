using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IActivityRepository : IDefaultRepository<Activity, ActivityFilter, Guid>
{
}
