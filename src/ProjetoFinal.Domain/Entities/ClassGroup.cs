using System;
using System.Collections.Generic;

namespace ProjetoFinal.Domain.Entities;

public class ClassGroup : AuditableEntity
{
    public Guid CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool RequiresApproval { get; set; }
    public bool RequiresEnrollmentCode { get; set; }
    public string? EnrollmentCodeHash { get; set; }
    public DateTime? EnrollmentOpensAt { get; set; }
    public DateTime? EnrollmentClosesAt { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool EnableChat { get; set; } = true;

    public Course? Course { get; set; }
    public ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();
    public ICollection<CourseContent> ScopedContents { get; set; } = new List<CourseContent>();
    public ICollection<ActivityAudience> ActivityAudiences { get; set; } = new List<ActivityAudience>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
