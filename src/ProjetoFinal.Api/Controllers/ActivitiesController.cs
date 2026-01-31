using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Activities;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjetoFinal.Api.Controllers;

[Route("api/activities")]
[Route("api/v1/activities")]
public class ActivitiesController : BaseController<
    ActivityDto,
    ActivityCreateDto,
    ActivityUpdateDto,
    ActivityFilter,
    Guid,
    IActivityAppService>
{
    private readonly ICourseAppService _courseService;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly IActivityRepository _activityRepository;

    public ActivitiesController(
        IActivityAppService service,
        ICourseAppService courseService,
        IClassGroupRepository classGroupRepository,
        IActivityRepository activityRepository) : base(service)
    {
        _courseService = courseService;
        _classGroupRepository = classGroupRepository;
        _activityRepository = activityRepository;
    }

    [HttpGet]
    public override async Task<PagedResultDto<ActivityDto>> GetAllAsync(
        [FromQuery] ActivityFilter filter,
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
    public override async Task<ActivityDto> GetByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsActivityAsync(id, cancellationToken);
        return await base.GetByIdAsync(id, cancellationToken);
    }

    [HttpPost]
    public override async Task<ActivityDto> AddAsync(
        [FromBody] ActivityCreateDto createDto,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsClassGroupAsync(createDto.ClassGroupId, cancellationToken);
        if (IsInstructor() && !IsAdministrator())
        {
            createDto.CreatedById = ResolveCurrentUserId();
        }

        return await base.AddAsync(createDto, cancellationToken);
    }

    [HttpPut("{id}")]
    public override async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] ActivityUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsActivityAsync(id, cancellationToken);
        return await base.UpdateAsync(id, dto, cancellationToken);
    }

    [HttpDelete("{id}")]
    public override async Task<ActivityDto> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsActivityAsync(id, cancellationToken);
        return await base.DeleteAsync(id, cancellationToken);
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

    private async Task EnsureInstructorOwnsClassGroupAsync(Guid classGroupId, CancellationToken cancellationToken)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return;
        }

        var classGroup = await _classGroupRepository.FindAsync(classGroupId, cancellationToken);
        if (classGroup is null)
        {
            throw new BusinessException("Turma nao encontrada.", ECodigo.NaoEncontrado);
        }

        await EnsureInstructorOwnsCourseAsync(classGroup.CourseId, cancellationToken);
    }

    private async Task EnsureInstructorOwnsCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return;
        }

        var instructorId = ResolveCurrentUserId();
        if (instructorId == Guid.Empty)
        {
            throw new BusinessException("Instrutor nao identificado.", ECodigo.NaoAutenticado);
        }

        var course = await _courseService.GetByIdAsync(courseId, cancellationToken);
        if (course.InstructorId != instructorId)
        {
            throw new BusinessException("Acesso nao permitido a atividade.", ECodigo.NaoPermitido);
        }
    }

    private async Task EnsureInstructorOwnsActivityAsync(Guid activityId, CancellationToken cancellationToken)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return;
        }

        var activity = await _activityRepository.FindAsync(activityId, cancellationToken);
        if (activity is null)
        {
            throw new BusinessException("Atividade nao encontrada.", ECodigo.NaoEncontrado);
        }

        await EnsureInstructorOwnsCourseAsync(activity.CourseId, cancellationToken);
    }
}
