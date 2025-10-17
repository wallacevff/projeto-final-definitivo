using System;
using ProjetoFinal.Application.Contracts.Dto.Users;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IUserAppService : IDefaultService<UserDto, UserCreateDto, UserUpdateDto, UserFilter, Guid>
{
    Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
