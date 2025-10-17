using System;
using System.Collections.Generic;

namespace ProjetoFinal.Domain.Entities;

public class ChatMessage : AuditableEntity
{
    public Guid ClassGroupId { get; set; }
    public Guid SenderId { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public Guid? MediaResourceId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsSystemMessage { get; set; }

    public ClassGroup? ClassGroup { get; set; }
    public User? Sender { get; set; }
    public ChatMessage? ReplyTo { get; set; }
    public MediaResource? MediaResource { get; set; }
    public ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();
}
