using System;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface ICourseCategoryAppService : IDefaultService<CourseCategoryDto, CourseCategoryCreateDto, CourseCategoryUpdateDto, CourseCategoryFilter, Guid>
{
}
