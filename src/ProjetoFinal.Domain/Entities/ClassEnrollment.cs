using System;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Domain.Entities;

public class ClassEnrollment : AuditableEntity
{
    public Guid ClassGroupId { get; set; }
    public Guid StudentId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecisionAt { get; set; }
    public Guid? DecidedById { get; set; }
    public string? DecisionReason { get; set; }

    public ClassGroup? ClassGroup { get; set; }
    public User? Student { get; set; }
    public User? DecidedBy { get; set; }
}
