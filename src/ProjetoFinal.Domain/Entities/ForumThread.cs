using System;
using System.Collections.Generic;

namespace ProjetoFinal.Domain.Entities;

public class ForumThread : AuditableEntity
{
    public Guid CourseId { get; set; }
    public Guid ClassGroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsLocked { get; set; }
    public bool IsPinned { get; set; }
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    public Course? Course { get; set; }
    public ClassGroup ClassGroup { get; set; } = null!;
    public User? CreatedBy { get; set; }
    public ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
}
