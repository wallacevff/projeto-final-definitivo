using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Forum;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[ApiController]
[Route("api/forum/posts")]
[Route("api/v1/forum/posts")]
public class ForumPostsController : ControllerBase
{
    private readonly IForumAppService _service;

    public ForumPostsController(IForumAppService service)
    {
        _service = service;
    }

    [HttpPost]
    public Task<ForumPostDto> CreateAsync(
        [FromBody] ForumPostCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        return _service.CreatePostAsync(dto, cancellationToken);
    }

    [HttpPut("{postId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid postId,
        [FromBody] ForumPostUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await _service.UpdatePostAsync(postId, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid postId,
        CancellationToken cancellationToken = default)
    {
        await _service.DeletePostAsync(postId, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public Task<PagedResultDto<ForumPostDto>> GetAllAsync(
        [FromQuery] ForumPostFilter filter,
        CancellationToken cancellationToken = default)
    {
        return _service.GetPostsAsync(filter, cancellationToken);
    }
}
