using System;

namespace ProjetoFinal.Application.Contracts.Dto.Forum;

public class ForumThreadCreateDto
{
    public Guid ClassGroupId { get; set; }
    public Guid CreatedById { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPinned { get; set; }
}
