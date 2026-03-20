using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Activities;

public class ActivitySubmissionUpdateDto
{
    public SubmissionStatus Status { get; set; }
    public decimal? Score { get; set; }
    public Guid? GradedById { get; set; }
    public string? Feedback { get; set; }
    public int? MasteryScore { get; set; }
    public int? ApplicationScore { get; set; }
    public int? CommunicationScore { get; set; }
    public string? FeedbackTags { get; set; }
    public string? RecommendedAction { get; set; }
    public string? TextAnswer { get; set; }
    public IList<SubmissionAttachmentCreateDto> Attachments { get; set; } = new List<SubmissionAttachmentCreateDto>();
}
