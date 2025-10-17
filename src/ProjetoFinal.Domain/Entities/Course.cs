using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Domain.Entities;

public class Course : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? DetailedDescription { get; set; }
    public CourseMode Mode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public Guid? ThumbnailMediaId { get; set; }
    public bool EnableForum { get; set; } = true;
    public bool EnableChat { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? EnrollmentInstructions { get; set; }

    public User? Instructor { get; set; }
    public MediaResource? ThumbnailMedia { get; set; }

    public ICollection<ClassGroup> ClassGroups { get; set; } = new List<ClassGroup>();
    public ICollection<CourseContent> Contents { get; set; } = new List<CourseContent>();
    public ICollection<ForumThread> ForumThreads { get; set; } = new List<ForumThread>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<CourseSubscription> Subscriptions { get; set; } = new List<CourseSubscription>();
}
