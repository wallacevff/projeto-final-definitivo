using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ActivityRepository(AppDbContext context)
    : DefaultRepository<Activity, ActivityFilter, Guid>(context), IActivityRepository
{
    protected override IQueryable<Activity> ApplyIncludes(IQueryable<Activity> query)
    {
        return query
            .Include(activity => activity.ClassGroup)
            .Include(activity => activity.Audiences)
                .ThenInclude(audience => audience.ClassGroup)
            .Include(activity => activity.Attachments);
    }

    protected override IQueryable<Activity> ApplyIncludesList(IQueryable<Activity> query)
    {
        return ApplyIncludes(query);
    }
}
