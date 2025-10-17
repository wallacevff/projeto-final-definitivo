using System;

namespace ProjetoFinal.Domain.Entities;

public class ForumPostAttachment : AuditableEntity
{
    public Guid ForumPostId { get; set; }
    public Guid MediaResourceId { get; set; }
    public string? Caption { get; set; }

    public ForumPost? ForumPost { get; set; }
    public MediaResource? MediaResource { get; set; }
}
