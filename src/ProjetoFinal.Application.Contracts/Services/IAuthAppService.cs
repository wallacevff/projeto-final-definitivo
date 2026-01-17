using ProjetoFinal.Application.Contracts.Dto.Auth;
using ProjetoFinal.Application.Contracts.Dto.Users;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IAuthAppService : IApplicationContracts
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
}
