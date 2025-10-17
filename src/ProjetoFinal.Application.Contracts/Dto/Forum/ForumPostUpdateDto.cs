using System;
using System.Collections.Generic;

namespace ProjetoFinal.Application.Contracts.Dto.Forum;

public class ForumPostUpdateDto
{
    public string Message { get; set; } = string.Empty;
    public IList<ForumPostAttachmentCreateDto> Attachments { get; set; } = new List<ForumPostAttachmentCreateDto>();
}
