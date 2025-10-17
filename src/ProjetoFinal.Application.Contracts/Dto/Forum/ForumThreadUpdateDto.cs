using System;

namespace ProjetoFinal.Application.Contracts.Dto.Forum;

public class ForumThreadUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsLocked { get; set; }
    public bool IsPinned { get; set; }
}
