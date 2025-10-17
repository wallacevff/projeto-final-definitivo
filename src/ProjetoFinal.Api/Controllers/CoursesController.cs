using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjetoFinal.Api.Controllers;

[Route("api/courses")]
[Route("api/v1/courses")]
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

    [HttpPost]
    public override async Task<CourseDto> AddAsync(
        [FromBody] CourseCreateDto createDto,
        CancellationToken cancellationToken = default)
    {
        var instructorId = ResolveCurrentUserId();
        if (instructorId == Guid.Empty)
        {
            throw new BusinessException("Instrutor nao identificado.", ECodigo.NaoAutenticado);
        }

        createDto.InstructorId = instructorId;
        return await base.AddAsync(createDto, cancellationToken);
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

    private Guid ResolveCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }
}
