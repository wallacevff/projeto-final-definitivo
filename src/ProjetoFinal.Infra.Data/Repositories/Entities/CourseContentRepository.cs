using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class CourseContentRepository(AppDbContext context)
    : DefaultRepository<CourseContent, CourseContentFilter, Guid>(context), ICourseContentRepository
{
}
