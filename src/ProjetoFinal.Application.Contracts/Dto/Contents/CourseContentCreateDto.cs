using System;
using System.Collections.Generic;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Contents;

public class CourseContentCreateDto
{
    public Guid CourseId { get; set; }
    public Guid? ClassGroupId { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public ContentItemType ItemType { get; set; }
    public bool IsDraft { get; set; }
    public int DisplayOrder { get; set; }
    public IList<ContentAttachmentCreateDto> Attachments { get; set; } = new List<ContentAttachmentCreateDto>();
}

public class ContentAttachmentCreateDto
{
    public Guid MediaResourceId { get; set; }
    public string? Caption { get; set; }
    public bool IsPrimary { get; set; }
}
