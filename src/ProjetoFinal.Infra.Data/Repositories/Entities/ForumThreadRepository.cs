using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ForumThreadRepository(AppDbContext context)
    : DefaultRepository<ForumThread, ForumThreadFilter, Guid>(context), IForumThreadRepository
{
    protected override IQueryable<ForumThread> ApplyIncludes(IQueryable<ForumThread> query)
    {
        return query
            .Include(thread => thread.ClassGroup)
            .Include(thread => thread.CreatedBy);
    }

    protected override IQueryable<ForumThread> ApplyIncludesList(IQueryable<ForumThread> query)
    {
        return ApplyIncludes(query);
    }
}
