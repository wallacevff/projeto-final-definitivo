using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IUserRepository : IDefaultRepository<User, UserFilter, Guid>
{
    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
