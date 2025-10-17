using System;

namespace ProjetoFinal.Application.Contracts.Dto.Chat;

public class ChatMessageCreateDto
{
    public Guid ClassGroupId { get; set; }
    public Guid SenderId { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public Guid? MediaResourceId { get; set; }
    public string Message { get; set; } = string.Empty;
}
