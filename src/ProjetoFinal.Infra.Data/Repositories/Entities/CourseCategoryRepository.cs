using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class CourseCategoryRepository(AppDbContext context)
    : DefaultRepository<CourseCategory, CourseCategoryFilter, Guid>(context), ICourseCategoryRepository
{
}
