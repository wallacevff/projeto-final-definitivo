using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Domain.Entities;

public class MediaResource : AuditableEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public MediaKind Kind { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public double? DurationInSeconds { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Sha256 { get; set; }

    public ICollection<ContentAttachment> ContentAttachments { get; set; } = new List<ContentAttachment>();
    public ICollection<ForumPostAttachment> ForumPostAttachments { get; set; } = new List<ForumPostAttachment>();
    public ICollection<ActivityAttachment> ActivityAttachments { get; set; } = new List<ActivityAttachment>();
    public ICollection<SubmissionAttachment> SubmissionAttachments { get; set; } = new List<SubmissionAttachment>();
}
