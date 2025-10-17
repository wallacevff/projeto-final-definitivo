using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Chat;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[Route("api/chat/messages")]
public class ChatMessagesController : ControllerBase
{
    private readonly IChatMessageAppService _service;

    public ChatMessagesController(IChatMessageAppService service)
    {
        _service = service;
    }

    [HttpPost]
    public Task<ChatMessageDto> SendAsync(
        [FromBody] ChatMessageCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        return _service.SendAsync(dto, cancellationToken);
    }

    [HttpPut("{messageId:guid}")]
    public Task<ChatMessageDto> UpdateAsync(
        [FromRoute] Guid messageId,
        [FromBody] ChatMessageUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        return _service.UpdateAsync(messageId, dto, cancellationToken);
    }

    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(messageId, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public Task<PagedResultDto<ChatMessageDto>> GetAllAsync(
        [FromQuery] ChatMessageFilter filter,
        CancellationToken cancellationToken = default)
    {
        return _service.GetMessagesAsync(filter, cancellationToken);
    }
}
