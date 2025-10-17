using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Domain.Entities;

public class CourseContent : AuditableEntity
{
    public Guid CourseId { get; set; }
    public Guid? ClassGroupId { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public ContentItemType ItemType { get; set; } = ContentItemType.Text;
    public bool IsDraft { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int DisplayOrder { get; set; }

    public Course? Course { get; set; }
    public ClassGroup? ClassGroup { get; set; }
    public User? Author { get; set; }
    public ICollection<ContentAttachment> Attachments { get; set; } = new List<ContentAttachment>();
}
