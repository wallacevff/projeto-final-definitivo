using ProjetoFinal.Application.Contracts.Dto.Auth;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IAuthAppService : IApplicationContracts
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
