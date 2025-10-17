using System;

namespace ProjetoFinal.Application.Contracts.Dto.Chat;

public class ChatMessageUpdateDto
{
    public string Message { get; set; } = string.Empty;
    public Guid? MediaResourceId { get; set; }
}
