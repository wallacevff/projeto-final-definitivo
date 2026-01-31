using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Enums;
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

    [HttpGet]
    public override async Task<PagedResultDto<CourseDto>> GetAllAsync(
        [FromQuery] CourseFilter filter,
        CancellationToken cancellationToken = default)
    {
        if (IsInstructor() && !IsAdministrator())
        {
            var instructorId = ResolveCurrentUserId();
            if (instructorId == Guid.Empty)
            {
                throw new BusinessException("Instrutor nao identificado.", ECodigo.NaoAutenticado);
            }

            filter.InstructorId = instructorId;
        }

        return await base.GetAllAsync(filter, cancellationToken);
    }

    [HttpGet("{id}")]
    public override async Task<CourseDto> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var course = await base.GetByIdAsync(id, cancellationToken);
        await EnsureInstructorOwnsCourseAsync(course, cancellationToken);
        return course;
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

    [HttpPut("{id}")]
    public override async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] CourseUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        var course = await base.GetByIdAsync(id, cancellationToken);
        await EnsureInstructorOwnsCourseAsync(course, cancellationToken);
        return await base.UpdateAsync(id, dto, cancellationToken);
    }

    [HttpDelete("{id}")]
    public override async Task<CourseDto> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var course = await base.GetByIdAsync(id, cancellationToken);
        await EnsureInstructorOwnsCourseAsync(course, cancellationToken);
        return await base.DeleteAsync(id, cancellationToken);
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

    private bool IsInstructor()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return string.Equals(role, nameof(UserRole.Instructor), StringComparison.OrdinalIgnoreCase);
    }

    private bool IsAdministrator()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return string.Equals(role, nameof(UserRole.Administrator), StringComparison.OrdinalIgnoreCase);
    }

    private Task EnsureInstructorOwnsCourseAsync(CourseDto course, CancellationToken cancellationToken)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return Task.CompletedTask;
        }

        var instructorId = ResolveCurrentUserId();
        if (instructorId == Guid.Empty)
        {
            throw new BusinessException("Instrutor nao identificado.", ECodigo.NaoAutenticado);
        }

        if (course.InstructorId != instructorId)
        {
            throw new BusinessException("Acesso nao permitido ao curso.", ECodigo.NaoPermitido);
        }

        return Task.CompletedTask;
    }
}
