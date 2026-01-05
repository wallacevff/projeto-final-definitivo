using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ForumPostRepository(AppDbContext context)
    : DefaultRepository<ForumPost, ForumPostFilter, Guid>(context), IForumPostRepository
{
    protected override IQueryable<ForumPost> ApplyIncludes(IQueryable<ForumPost> query)
    {
        return query
            .Include(post => post.Author)
            .Include(post => post.Attachments)
                .ThenInclude(attachment => attachment.MediaResource);
    }

    protected override IQueryable<ForumPost> ApplyIncludesList(IQueryable<ForumPost> query)
    {
        return ApplyIncludes(query);
    }
}
