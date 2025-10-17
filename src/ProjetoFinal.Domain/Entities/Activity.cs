using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Domain.Entities;

public class Activity : AuditableEntity
{
    public Guid CourseId { get; set; }
    public Guid? ReferenceContentId { get; set; }
    public ActivityScope Scope { get; set; } = ActivityScope.Course;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? AvailableAt { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? MaxScore { get; set; }
    public bool AllowLateSubmissions { get; set; }
    public int? LatePenaltyPercentage { get; set; }
    public bool VisibleToStudents { get; set; } = true;

    public Course? Course { get; set; }
    public User? CreatedBy { get; set; }
    public CourseContent? ReferenceContent { get; set; }
    public ICollection<ActivityAudience> Audiences { get; set; } = new List<ActivityAudience>();
    public ICollection<ActivityAttachment> Attachments { get; set; } = new List<ActivityAttachment>();
    public ICollection<ActivitySubmission> Submissions { get; set; } = new List<ActivitySubmission>();
}
