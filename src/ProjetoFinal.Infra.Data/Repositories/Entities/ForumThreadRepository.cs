using System;
using System.Linq;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;
using System.Linq.Expressions;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ForumThreadRepository(AppDbContext context)
    : DefaultRepository<ForumThread, ForumThreadFilter, Guid>(context), IForumThreadRepository
{
    protected override IQueryable<ForumThread> ApplyIncludes(IQueryable<ForumThread> query)
    {
        return query
            .Include(thread => thread.Course)
            .Include(thread => thread.ClassGroup)
            .Include(thread => thread.CreatedBy);
    }

    protected override IQueryable<ForumThread> ApplyIncludesList(IQueryable<ForumThread> query)
    {
        return ApplyIncludes(query);
    }

    protected override Expression<Func<ForumThread, bool>> GetFilters(ForumThreadFilter filter)
    {
        var predicate = base.GetFilters(filter);

        if (filter.InstructorId is not null && filter.InstructorId != Guid.Empty)
        {
            var instructorId = filter.InstructorId.Value;
            predicate = predicate.And(thread =>
                thread.Course != null && thread.Course.InstructorId == instructorId);
        }

        return predicate;
    }
}
