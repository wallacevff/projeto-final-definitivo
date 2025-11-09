using System;
using System.Linq;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class CourseContentRepository(AppDbContext context)
    : DefaultRepository<CourseContent, CourseContentFilter, Guid>(context), ICourseContentRepository
{
    protected override IQueryable<CourseContent> ApplyIncludes(IQueryable<CourseContent> query)
    {
        return query.Include(content => content.Attachments)
            .ThenInclude(attachment => attachment.MediaResource);
    }

    protected override IQueryable<CourseContent> ApplyIncludesList(IQueryable<CourseContent> query)
    {
        return ApplyIncludes(query);
    }
}
