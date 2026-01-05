using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Forum;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
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

    public ForumThreadsController(IForumAppService service)
    {
        _service = service;
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
        return _service.CreateThreadAsync(dto, cancellationToken);
    }

    [HttpPut("{threadId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid threadId,
        [FromBody] ForumThreadUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await _service.UpdateThreadAsync(threadId, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{threadId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid threadId,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteThreadAsync(threadId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{threadId:guid}")]
    public Task<ForumThreadDto> GetByIdAsync(
        [FromRoute] Guid threadId,
        CancellationToken cancellationToken = default)
    {
        return _service.GetThreadByIdAsync(threadId, cancellationToken);
    }

    [HttpGet]
    public Task<PagedResultDto<ForumThreadDto>> GetAllAsync(
        [FromQuery] ForumThreadFilter filter,
        CancellationToken cancellationToken = default)
    {
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
}
