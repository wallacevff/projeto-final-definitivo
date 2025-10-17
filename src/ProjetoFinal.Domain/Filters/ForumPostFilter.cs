using System;
using ProjetoFinal.Domain.Shared.Filters;

namespace ProjetoFinal.Domain.Filters;

public class ForumPostFilter : Filter
{
    public Guid? ThreadId { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? ParentPostId { get; set; }
}
