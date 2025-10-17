using System;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Users;

public class UserUpdateDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}
