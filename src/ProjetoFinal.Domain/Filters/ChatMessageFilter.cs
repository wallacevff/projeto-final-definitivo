using System;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class ChatMessageFilter : Filter
{
    public Guid? ClassGroupId { get; set; }
    public Guid? SenderId { get; set; }
    public Guid? RecipientId { get; set; }
    public Guid? CurrentUserId { get; set; }
    public DateTime? SentFrom { get; set; }
    public DateTime? SentTo { get; set; }
}
