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
    public string? TextAnswer { get; set; }
    public IList<SubmissionAttachmentCreateDto> Attachments { get; set; } = new List<SubmissionAttachmentCreateDto>();
}
