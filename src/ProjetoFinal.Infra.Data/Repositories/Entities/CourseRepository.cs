using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class CourseRepository : DefaultRepository<Course, CourseFilter, Guid>, ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _context.Courses.AnyAsync(p => p.Slug == slug, cancellationToken);
    }

    protected override IQueryable<Course> ApplyIncludes(IQueryable<Course> query)
    {
        return query
            .Include(course => course.Instructor)
            .Include(course => course.ClassGroups)
                .ThenInclude(group => group.Enrollments);
    }

    protected override IQueryable<Course> ApplyIncludesList(IQueryable<Course> query)
    {
        return ApplyIncludes(query);
    }
}
