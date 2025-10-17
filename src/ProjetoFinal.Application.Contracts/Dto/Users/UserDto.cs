using System;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public Guid ExternalId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Biography { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
}
