using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjetoFinal.Application.Contracts.Dto.Auth;
using ProjetoFinal.Application.Contracts.Dto.Users;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using ProjetoFinal.Domain.Shared.Security;
using ProjetoFinal.Infra.CrossCutting.ConfigurationModels;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Auth;

public class AuthAppService : IAuthAppService, IApplicationServices
{
    private readonly IUserRepository _userRepository;
    private readonly IUnityOfWork _unityOfWork;
    private readonly IAutomapApi _mapper;
    private readonly JwtConfiguration _jwtConfiguration;

    public AuthAppService(
        IUserRepository userRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper,
        IOptions<JwtConfiguration> jwtOptions)
    {
        _userRepository = userRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
        _jwtConfiguration = jwtOptions.Value;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BusinessException("Credenciais invalidas.", ECodigo.NaoAutenticado);
        }

        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
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

    public async Task<UserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new BusinessException("Dados de cadastro invalidos.", ECodigo.MaRequisicao);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(request.FullName)
            || string.IsNullOrWhiteSpace(normalizedEmail)
            || string.IsNullOrWhiteSpace(normalizedUsername)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BusinessException("Preencha todos os campos obrigatorios.", ECodigo.MaRequisicao);
        }

        if (request.Password.Length < 6)
        {
            throw new BusinessException("A senha deve possuir pelo menos 6 caracteres.", ECodigo.MaRequisicao);
        }

        if (request.Role is not UserRole.Student and not UserRole.Instructor)
        {
            throw new BusinessException("Perfil de usuario invalido.", ECodigo.MaRequisicao);
        }

        var existingUser = await _userRepository.FirstOrDefaultByPredicateAsync(
            user => user.Email == normalizedEmail || user.Username == normalizedUsername,
            cancellationToken);

        if (existingUser is not null)
        {
            throw new BusinessException("Ja existe um usuario cadastrado com este e-mail ou usuario.", ECodigo.Conflito);
        }

        var user = new User
        {
            ExternalId = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Username = normalizedUsername,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = request.Role,
            Biography = string.IsNullOrWhiteSpace(request.Biography) ? null : request.Biography.Trim(),
            AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim(),
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.MapFrom<UserDto>(user);
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
