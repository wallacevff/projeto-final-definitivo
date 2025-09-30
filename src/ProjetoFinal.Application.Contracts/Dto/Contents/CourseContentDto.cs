using System;
using System.Collections.Generic;
using ProjetoFinal.Application.Contracts.Dto.Media;
using ProjetoFinal.Domain.Enums;

namespace ProjetoFinal.Application.Contracts.Dto.Contents;

public class CourseContentDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid? ClassGroupId { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public ContentItemType ItemType { get; set; }
    public bool IsDraft { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime? PublishedAt { get; set; }

    public IList<ContentAttachmentDto> Attachments { get; set; } = new List<ContentAttachmentDto>();
}

public class ContentAttachmentDto
{
    public Guid Id { get; set; }
    public Guid CourseContentId { get; set; }
    public Guid MediaResourceId { get; set; }
    public string? Caption { get; set; }
    public bool IsPrimary { get; set; }
    public MediaResourceDto? Media { get; set; }
}
