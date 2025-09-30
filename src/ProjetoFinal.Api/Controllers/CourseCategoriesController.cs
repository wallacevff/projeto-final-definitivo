using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[Route("api/course-categories")]
public class CourseCategoriesController : BaseController<
    CourseCategoryDto,
    CourseCategoryCreateDto,
    CourseCategoryUpdateDto,
    CourseCategoryFilter,
    Guid,
    ICourseCategoryAppService>
{
    public CourseCategoriesController(ICourseCategoryAppService service) : base(service)
    {
    }
}
