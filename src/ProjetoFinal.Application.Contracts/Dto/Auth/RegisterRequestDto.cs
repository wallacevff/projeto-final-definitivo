using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Auth;

public class RegisterRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Biography { get; set; }
    public string? AvatarUrl { get; set; }
}
