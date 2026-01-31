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

public class ActivityRepository(AppDbContext context)
    : DefaultRepository<Activity, ActivityFilter, Guid>(context), IActivityRepository
{
    protected override IQueryable<Activity> ApplyIncludes(IQueryable<Activity> query)
    {
        return query
            .Include(activity => activity.Course)
            .Include(activity => activity.ClassGroup)
            .Include(activity => activity.Audiences)
                .ThenInclude(audience => audience.ClassGroup)
            .Include(activity => activity.Attachments);
    }

    protected override IQueryable<Activity> ApplyIncludesList(IQueryable<Activity> query)
    {
        return ApplyIncludes(query);
    }

    protected override Expression<Func<Activity, bool>> GetFilters(ActivityFilter filter)
    {
        var predicate = base.GetFilters(filter);

        if (filter.InstructorId is not null && filter.InstructorId != Guid.Empty)
        {
            var instructorId = filter.InstructorId.Value;
            predicate = predicate.And(activity =>
                activity.Course != null && activity.Course.InstructorId == instructorId);
        }

        return predicate;
    }
}
