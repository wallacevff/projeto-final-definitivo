using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Activities;

public class ActivityCreateDto
{
    public Guid CourseId { get; set; }
    public Guid CreatedById { get; set; }
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
    public IList<Guid> ClassGroupIds { get; set; } = new List<Guid>();
    public IList<ActivityAttachmentCreateDto> Attachments { get; set; } = new List<ActivityAttachmentCreateDto>();
}

public class ActivityAttachmentCreateDto
{
    public Guid MediaResourceId { get; set; }
    public string? Caption { get; set; }
}
