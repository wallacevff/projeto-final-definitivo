using System;

namespace ProjetoFinal.Application.Contracts.Dto.Chat;

public class ChatPresenceUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}
