using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class UserFilter : Filter
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}
