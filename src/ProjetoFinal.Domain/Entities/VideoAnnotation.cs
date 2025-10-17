using System;

namespace ProjetoFinal.Domain.Entities;

public class VideoAnnotation : AuditableEntity
{
    public Guid SubmissionId { get; set; }
    public Guid AttachmentId { get; set; }
    public double TimeMarkerSeconds { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime? EditedAt { get; set; }

    public ActivitySubmission? Submission { get; set; }
    public SubmissionAttachment? Attachment { get; set; }
    public User? CreatedBy { get; set; }
}
