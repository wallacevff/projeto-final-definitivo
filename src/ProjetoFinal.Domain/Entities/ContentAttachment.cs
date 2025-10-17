using System;

namespace ProjetoFinal.Domain.Entities;

public class ContentAttachment : AuditableEntity
{
    public Guid CourseContentId { get; set; }
    public Guid MediaResourceId { get; set; }
    public string? Caption { get; set; }
    public bool IsPrimary { get; set; }

    public CourseContent? CourseContent { get; set; }
    public MediaResource? MediaResource { get; set; }
}
