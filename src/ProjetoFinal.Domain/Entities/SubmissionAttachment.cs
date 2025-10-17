using System;

namespace ProjetoFinal.Domain.Entities;

public class SubmissionAttachment : AuditableEntity
{
    public Guid SubmissionId { get; set; }
    public Guid MediaResourceId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsVideo { get; set; }

    public ActivitySubmission? Submission { get; set; }
    public MediaResource? MediaResource { get; set; }
}
