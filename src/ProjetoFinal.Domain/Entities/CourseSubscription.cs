using System;

namespace ProjetoFinal.Domain.Entities;

public class CourseSubscription : AuditableEntity
{
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public Course? Course { get; set; }
    public User? Student { get; set; }
}
