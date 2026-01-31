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

public class ForumPostRepository(AppDbContext context)
    : DefaultRepository<ForumPost, ForumPostFilter, Guid>(context), IForumPostRepository
{
    protected override IQueryable<ForumPost> ApplyIncludes(IQueryable<ForumPost> query)
    {
        return query
            .Include(post => post.Author)
            .Include(post => post.Thread!)
                .ThenInclude(thread => thread.Course)
            .Include(post => post.Attachments)
                .ThenInclude(attachment => attachment.MediaResource);
    }

    protected override IQueryable<ForumPost> ApplyIncludesList(IQueryable<ForumPost> query)
    {
        return ApplyIncludes(query);
    }

    protected override Expression<Func<ForumPost, bool>> GetFilters(ForumPostFilter filter)
    {
        var predicate = base.GetFilters(filter);

        if (filter.InstructorId is not null && filter.InstructorId != Guid.Empty)
        {
            var instructorId = filter.InstructorId.Value;
            predicate = predicate.And(post =>
                post.Thread != null &&
                post.Thread.Course != null &&
                post.Thread.Course.InstructorId == instructorId);
        }

        return predicate;
    }
}
