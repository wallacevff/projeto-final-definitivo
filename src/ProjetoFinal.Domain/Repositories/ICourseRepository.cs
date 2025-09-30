using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface ICourseRepository : IDefaultRepository<Course, CourseFilter, Guid>
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
}
