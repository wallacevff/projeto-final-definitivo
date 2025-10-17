using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjetoFinal.Application.Contracts.Dto.Auth;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using ProjetoFinal.Domain.Shared.Security;
using ProjetoFinal.Infra.CrossCutting.ConfigurationModels;

namespace ProjetoFinal.Aplication.Services.Services.Auth;

public class AuthAppService : IAuthAppService, IApplicationServices
{
    private readonly IUserRepository _userRepository;
    private readonly JwtConfiguration _jwtConfiguration;

    public AuthAppService(
        IUserRepository userRepository,
        IOptions<JwtConfiguration> jwtOptions)
    {
        _userRepository = userRepository;
        _jwtConfiguration = jwtOptions.Value;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BusinessException("Credenciais invalidas.", ECodigo.NaoAutenticado);
        }

        var normalizedUsername = request.Username.Trim();
        var user = await _userRepository.FindByUsernameAsync(normalizedUsername, cancellationToken);

        if (user is null || !user.IsActive || !PasswordHasher.Verify(request.Password, user.PasswordHash ?? string.Empty))
        {
            throw new BusinessException("Credenciais invalidas.", ECodigo.NaoAutenticado);
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtConfiguration.ExpiresInMinutes);
        var token = GenerateToken(user, expiresAt);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new AuthenticatedUserDto
            {
                Id = user.Id,
                ExternalId = user.ExternalId,
                Username = user.Username ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    private string GenerateToken(User user, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username ?? user.Email),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.ExternalId != Guid.Empty)
        {
            claims.Add(new Claim("external_id", user.ExternalId.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtConfiguration.Issuer,
            audience: _jwtConfiguration.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
