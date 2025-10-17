using System;
using ProjetoFinal.Application.Contracts.Dto.Media;

namespace ProjetoFinal.Application.Contracts.Dto.Chat;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ClassGroupId { get; set; }
    public Guid SenderId { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public Guid? MediaResourceId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsSystemMessage { get; set; }
    public MediaResourceDto? Media { get; set; }
}
