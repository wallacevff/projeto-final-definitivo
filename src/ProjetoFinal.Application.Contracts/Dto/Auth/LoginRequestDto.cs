namespace ProjetoFinal.Application.Contracts.Dto.Auth;

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
