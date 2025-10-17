using System;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AuthenticatedUserDto User { get; set; } = new();
}

public class AuthenticatedUserDto
{
    public Guid Id { get; set; }
    public Guid ExternalId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
