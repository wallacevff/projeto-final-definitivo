using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Forum;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;

namespace ProjetoFinal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/forum/threads")]
[Route("api/v1/forum/threads")]
public class ForumThreadsController : ControllerBase
{
    private readonly IForumAppService _service;
    private readonly ICourseAppService _courseService;
    private readonly IClassGroupRepository _classGroupRepository;

    public ForumThreadsController(
        IForumAppService service,
        ICourseAppService courseService,
        IClassGroupRepository classGroupRepository)
    {
        _service = service;
        _courseService = courseService;
        _classGroupRepository = classGroupRepository;
    }

    [HttpPost]
    public Task<ForumThreadDto> CreateAsync(
        [FromBody] ForumThreadCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveCurrentUserId();
        if (userId == Guid.Empty)
        {
            throw new BusinessException("Usuario nao identificado.", ECodigo.NaoAutenticado);
        }

        if (!IsInstructor())
        {
            throw new BusinessException("Apenas instrutores podem criar topicos.", ECodigo.NaoPermitido);
        }

        dto.CreatedById = userId;
        return CreateThreadInternalAsync(dto, cancellationToken);
    }

    [HttpPut("{threadId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid threadId,
        [FromBody] ForumThreadUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsThreadAsync(threadId, cancellationToken);
        await _service.UpdateThreadAsync(threadId, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{threadId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid threadId,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsThreadAsync(threadId, cancellationToken);
        await _service.DeleteThreadAsync(threadId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{threadId:guid}")]
    public Task<ForumThreadDto> GetByIdAsync(
        [FromRoute] Guid threadId,
        CancellationToken cancellationToken = default)
    {
        return GetThreadInternalAsync(threadId, cancellationToken);
    }

    [HttpGet]
    public Task<PagedResultDto<ForumThreadDto>> GetAllAsync(
        [FromQuery] ForumThreadFilter filter,
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

        return _service.GetThreadsAsync(filter, cancellationToken);
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }

    private bool IsInstructor()
    {
        var roleValue = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(roleValue))
        {
            return false;
        }

        if (Enum.TryParse<UserRole>(roleValue, ignoreCase: true, out var role))
        {
            return role == UserRole.Instructor;
        }

        if (int.TryParse(roleValue, out var numericRole))
        {
            return numericRole == (int)UserRole.Instructor;
        }

        return false;
    }

    private bool IsAdministrator()
    {
        var roleValue = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(roleValue))
        {
            return false;
        }

        if (Enum.TryParse<UserRole>(roleValue, ignoreCase: true, out var role))
        {
            return role == UserRole.Administrator;
        }

        if (int.TryParse(roleValue, out var numericRole))
        {
            return numericRole == (int)UserRole.Administrator;
        }

        return false;
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
            throw new BusinessException("Acesso nao permitido ao topico.", ECodigo.NaoPermitido);
        }
    }

    private async Task EnsureInstructorOwnsThreadAsync(Guid threadId, CancellationToken cancellationToken)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return;
        }

        var thread = await _service.GetThreadByIdAsync(threadId, cancellationToken);
        await EnsureInstructorOwnsCourseAsync(thread.CourseId, cancellationToken);
    }

    private async Task<ForumThreadDto> GetThreadInternalAsync(Guid threadId, CancellationToken cancellationToken)
    {
        var thread = await _service.GetThreadByIdAsync(threadId, cancellationToken);
        await EnsureInstructorOwnsCourseAsync(thread.CourseId, cancellationToken);
        return thread;
    }

    private async Task<ForumThreadDto> CreateThreadInternalAsync(ForumThreadCreateDto dto, CancellationToken cancellationToken)
    {
        if (IsInstructor() && !IsAdministrator())
        {
            var classGroup = await _classGroupRepository.FindAsync(dto.ClassGroupId, cancellationToken);
            if (classGroup is null)
            {
                throw new BusinessException("Turma nao encontrada para criar o topico.", ECodigo.NaoEncontrado);
            }

            await EnsureInstructorOwnsCourseAsync(classGroup.CourseId, cancellationToken);
        }

        return await _service.CreateThreadAsync(dto, cancellationToken);
    }
}
