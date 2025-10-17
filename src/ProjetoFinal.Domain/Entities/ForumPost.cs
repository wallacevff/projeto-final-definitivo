using System;
using System.Collections.Generic;

namespace ProjetoFinal.Domain.Entities;

public class ForumPost : AuditableEntity
{
    public Guid ThreadId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ParentPostId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? EditedAt { get; set; }

    public ForumThread? Thread { get; set; }
    public User? Author { get; set; }
    public ForumPost? ParentPost { get; set; }
    public ICollection<ForumPost> Replies { get; set; } = new List<ForumPost>();
    public ICollection<ForumPostAttachment> Attachments { get; set; } = new List<ForumPostAttachment>();
}
