using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Contents;

public class CourseContentUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public ContentItemType ItemType { get; set; }
    public bool IsDraft { get; set; }
    public int DisplayOrder { get; set; }
    public IList<ContentAttachmentCreateDto> Attachments { get; set; } = new List<ContentAttachmentCreateDto>();
}
