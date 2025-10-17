using System;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Courses;

public class CourseCategoryAppService : DefaultService<CourseCategory, CourseCategoryDto, CourseCategoryCreateDto, CourseCategoryUpdateDto, CourseCategoryFilter, Guid>, ICourseCategoryAppService
{
    public CourseCategoryAppService(
        ICourseCategoryRepository repository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
        : base(repository, unityOfWork, mapper)
    {
    }
}
