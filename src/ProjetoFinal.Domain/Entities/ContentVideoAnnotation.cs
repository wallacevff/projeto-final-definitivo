using System;

namespace ProjetoFinal.Domain.Entities;

public class ContentVideoAnnotation : AuditableEntity
{
    public Guid ContentAttachmentId { get; set; }
    public double TimeMarkerSeconds { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime? EditedAt { get; set; }

    public ContentAttachment? ContentAttachment { get; set; }
    public User? CreatedBy { get; set; }
}
