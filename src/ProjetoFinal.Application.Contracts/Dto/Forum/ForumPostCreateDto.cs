using System;
using System.Collections.Generic;

namespace ProjetoFinal.Application.Contracts.Dto.Forum;

public class ForumPostCreateDto
{
    public Guid ThreadId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ParentPostId { get; set; }
    public string Message { get; set; } = string.Empty;
    public IList<ForumPostAttachmentCreateDto> Attachments { get; set; } = new List<ForumPostAttachmentCreateDto>();
}

public class ForumPostAttachmentCreateDto
{
    public Guid MediaResourceId { get; set; }
    public string? Caption { get; set; }
}
