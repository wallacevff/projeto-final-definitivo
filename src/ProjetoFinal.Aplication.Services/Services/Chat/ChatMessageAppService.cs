using System;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Chat;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Chat;

public class ChatMessageAppService : IChatMessageAppService
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IAutomapApi _mapper;
    private readonly IUnityOfWork _unityOfWork;

    public ChatMessageAppService(
        IChatMessageRepository chatMessageRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
    {
        _chatMessageRepository = chatMessageRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
    }

    public async Task<ChatMessageDto> SendAsync(ChatMessageCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.MapFrom<ChatMessage>(dto);
        entity.SentAt = DateTime.UtcNow;
        var created = await _chatMessageRepository.AddAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<ChatMessageDto>(created);
    }

    public async Task<ChatMessageDto> UpdateAsync(Guid messageId, ChatMessageUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var message = await _chatMessageRepository.FindAsync(messageId, cancellationToken);
        if (message is null)
        {
            throw new BusinessException("Mensagem nao encontrada.", ECodigo.NaoEncontrado);
        }

        _mapper.MapTo(dto, message);
        message.UpdatedAt = DateTime.UtcNow;
        await _chatMessageRepository.UpdateAsync(message, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<ChatMessageDto>(message);
    }

    public async Task DeleteAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _chatMessageRepository.FindAsync(messageId, cancellationToken);
        if (message is null)
        {
            throw new BusinessException("Mensagem nao encontrada.", ECodigo.NaoEncontrado);
        }

        await _chatMessageRepository.DeleteAsync(message, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResultDto<ChatMessageDto>> GetMessagesAsync(ChatMessageFilter filter, CancellationToken cancellationToken = default)
    {
        var result = await _chatMessageRepository.GetAllAsync(filter, cancellationToken);
        return _mapper.MapFrom<PagedResultDto<ChatMessageDto>>(result);
    }
}
