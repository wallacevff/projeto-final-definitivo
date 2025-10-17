using System;
using System.Collections.Generic;

namespace ProjetoFinal.Application.Contracts.Dto.Activities;

public class ActivitySubmissionCreateDto
{
    public Guid ActivityId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? ClassGroupId { get; set; }
    public string? TextAnswer { get; set; }
    public IList<SubmissionAttachmentCreateDto> Attachments { get; set; } = new List<SubmissionAttachmentCreateDto>();
}

public class SubmissionAttachmentCreateDto
{
    public Guid MediaResourceId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsVideo { get; set; }
}
