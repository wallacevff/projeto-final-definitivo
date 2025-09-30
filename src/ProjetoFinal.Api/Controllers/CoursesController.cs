using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[Route("api/courses")]
public class CoursesController : BaseController<
    CourseDto,
    CourseCreateDto,
    CourseUpdateDto,
    CourseFilter,
    Guid,
    ICourseAppService>
{
    public CoursesController(ICourseAppService service) : base(service)
    {
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<CourseSummaryDto>> GetBySlugAsync(
        [FromRoute] string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest("Slug invalido.");
        }

        var course = await Service.GetBySlugAsync(slug, cancellationToken);
        return course is null ? NotFound() : Ok(course);
    }
}
