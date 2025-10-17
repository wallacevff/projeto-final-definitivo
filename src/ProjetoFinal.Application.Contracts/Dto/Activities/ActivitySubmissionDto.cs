using System;
using System.Collections.Generic;
using ProjetoFinal.Application.Contracts.Dto.Media;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Activities;

public class ActivitySubmissionDto
{
    public Guid Id { get; set; }
    public Guid ActivityId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? ClassGroupId { get; set; }
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public Guid? GradedById { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public string? TextAnswer { get; set; }

    public IList<SubmissionAttachmentDto> Attachments { get; set; } = new List<SubmissionAttachmentDto>();
    public IList<VideoAnnotationDto> VideoAnnotations { get; set; } = new List<VideoAnnotationDto>();
}

public class SubmissionAttachmentDto
{
    public Guid Id { get; set; }
    public Guid MediaResourceId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsVideo { get; set; }
    public MediaResourceDto? Media { get; set; }
}

public class VideoAnnotationDto
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid AttachmentId { get; set; }
    public Guid CreatedById { get; set; }
    public double TimeMarkerSeconds { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime? EditedAt { get; set; }
}
