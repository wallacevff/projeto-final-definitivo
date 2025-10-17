using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Domain.Entities;

public class ActivitySubmission : AuditableEntity
{
    public Guid ActivityId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? ClassGroupId { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? GradedAt { get; set; }
    public Guid? GradedById { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public string? TextAnswer { get; set; }

    public Activity? Activity { get; set; }
    public ClassGroup? ClassGroup { get; set; }
    public User? Student { get; set; }
    public User? GradedBy { get; set; }
    public ICollection<SubmissionAttachment> Attachments { get; set; } = new List<SubmissionAttachment>();
    public ICollection<VideoAnnotation> VideoAnnotations { get; set; } = new List<VideoAnnotation>();
}
