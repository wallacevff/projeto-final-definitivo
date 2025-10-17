using System;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Chat;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IChatMessageAppService
{
    Task<ChatMessageDto> SendAsync(ChatMessageCreateDto dto, CancellationToken cancellationToken = default);
    Task<ChatMessageDto> UpdateAsync(Guid messageId, ChatMessageUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ChatMessageDto>> GetMessagesAsync(ChatMessageFilter filter, CancellationToken cancellationToken = default);
}
