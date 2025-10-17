using System;
using System.Collections.Generic;
using ProjetoFinal.Application.Contracts.Dto.Media;

namespace ProjetoFinal.Application.Contracts.Dto.Forum;

public class ForumThreadDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid ClassGroupId { get; set; }
    public string ClassGroupName { get; set; } = string.Empty;
    public Guid CreatedById { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsLocked { get; set; }
    public bool IsPinned { get; set; }
    public DateTime LastActivityAt { get; set; }

    public IList<ForumPostDto> Posts { get; set; } = new List<ForumPostDto>();
}

public class ForumPostDto
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ParentPostId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }

    public IList<ForumPostAttachmentDto> Attachments { get; set; } = new List<ForumPostAttachmentDto>();
    public IList<ForumPostDto> Replies { get; set; } = new List<ForumPostDto>();
}

public class ForumPostAttachmentDto
{
    public Guid Id { get; set; }
    public Guid MediaResourceId { get; set; }
    public string? Caption { get; set; }
    public MediaResourceDto? Media { get; set; }
}
