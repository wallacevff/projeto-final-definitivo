using System;
using System.Collections.Generic;
using ProjetoFinal.Application.Contracts.Dto.ClassGroups;
using ProjetoFinal.Application.Contracts.Dto.Media;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Activities;

public class ActivityDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid ClassGroupId { get; set; }
    public string ClassGroupName { get; set; } = string.Empty;
    public Guid? ReferenceContentId { get; set; }
    public ActivityScope Scope { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? AvailableAt { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? MaxScore { get; set; }
    public bool AllowLateSubmissions { get; set; }
    public int? LatePenaltyPercentage { get; set; }
    public bool VisibleToStudents { get; set; }
    public Guid CreatedById { get; set; }

    public IList<ActivityAudienceDto> Audiences { get; set; } = new List<ActivityAudienceDto>();
    public IList<ActivityAttachmentDto> Attachments { get; set; } = new List<ActivityAttachmentDto>();
}

public class ActivityAudienceDto
{
    public Guid ClassGroupId { get; set; }
    public string ClassGroupName { get; set; } = string.Empty;
}

public class ActivityAttachmentDto
{
    public Guid Id { get; set; }
    public Guid MediaResourceId { get; set; }
    public string? Caption { get; set; }
    public MediaResourceDto? Media { get; set; }
}
