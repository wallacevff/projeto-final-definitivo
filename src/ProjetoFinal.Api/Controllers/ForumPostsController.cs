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
[Route("api/forum/posts")]
[Route("api/v1/forum/posts")]
public class ForumPostsController : ControllerBase
{
    private readonly IForumAppService _service;
    private readonly ICourseAppService _courseService;
    private readonly IForumPostRepository _postRepository;
    private readonly IForumThreadRepository _threadRepository;

    public ForumPostsController(
        IForumAppService service,
        ICourseAppService courseService,
        IForumPostRepository postRepository,
        IForumThreadRepository threadRepository)
    {
        _service = service;
        _courseService = courseService;
        _postRepository = postRepository;
        _threadRepository = threadRepository;
    }

    [HttpPost]
    public Task<ForumPostDto> CreateAsync(
        [FromBody] ForumPostCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveCurrentUserId();
        if (userId == Guid.Empty)
        {
            throw new BusinessException("Usuario nao identificado.", ECodigo.NaoAutenticado);
        }

        dto.AuthorId = userId;
        return CreatePostInternalAsync(dto, cancellationToken);
    }

    [HttpPut("{postId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid postId,
        [FromBody] ForumPostUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsPostAsync(postId, cancellationToken);
        await _service.UpdatePostAsync(postId, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default)
    {
        await EnsureInstructorOwnsPostAsync(postId, cancellationToken);
        await _service.DeletePostAsync(postId, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public Task<PagedResultDto<ForumPostDto>> GetAllAsync(
        [FromQuery] ForumPostFilter filter,
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

        return _service.GetPostsAsync(filter, cancellationToken);
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

    private async Task EnsureInstructorOwnsPostAsync(Guid postId, CancellationToken cancellationToken)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return;
        }

        var post = await _postRepository.FindAsync(postId, cancellationToken);
        if (post is null)
        {
            throw new BusinessException("Mensagem nao encontrada.", ECodigo.NaoEncontrado);
        }

        var thread = await _threadRepository.FindAsync(post.ThreadId, cancellationToken);
        if (thread is null)
        {
            throw new BusinessException("Topico nao encontrado.", ECodigo.NaoEncontrado);
        }

        await EnsureInstructorOwnsCourseAsync(thread.CourseId, cancellationToken);
    }

    private async Task<ForumPostDto> CreatePostInternalAsync(ForumPostCreateDto dto, CancellationToken cancellationToken)
    {
        if (IsInstructor() && !IsAdministrator())
        {
            var thread = await _threadRepository.FindAsync(dto.ThreadId, cancellationToken);
            if (thread is null)
            {
                throw new BusinessException("Topico nao encontrado.", ECodigo.NaoEncontrado);
            }

            await EnsureInstructorOwnsCourseAsync(thread.CourseId, cancellationToken);
        }

        return await _service.CreatePostAsync(dto, cancellationToken);
    }
}
