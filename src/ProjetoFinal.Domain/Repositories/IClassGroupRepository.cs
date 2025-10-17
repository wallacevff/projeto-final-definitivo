using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IClassGroupRepository : IDefaultRepository<ClassGroup, ClassGroupFilter, Guid>
{
    Task<bool> HasAvailableSeatsAsync(Guid classGroupId, CancellationToken cancellationToken = default);
}
