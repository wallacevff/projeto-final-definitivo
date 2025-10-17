using System;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface ICourseAppService : IDefaultService<CourseDto, CourseCreateDto, CourseUpdateDto, CourseFilter, Guid>
{
    Task<CourseSummaryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
