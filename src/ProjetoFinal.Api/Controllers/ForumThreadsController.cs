using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Forum;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[ApiController]
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
}
