using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Domain.Entities;

public class User : AuditableEntity
{
    public Guid ExternalId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public string? Biography { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Course> CoursesAsInstructor { get; set; } = new List<Course>();
    public ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
    public ICollection<CourseSubscription> CourseSubscriptions { get; set; } = new List<CourseSubscription>();
}
