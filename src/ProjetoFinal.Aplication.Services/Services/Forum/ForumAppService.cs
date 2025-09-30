using System;
using System.Collections.Generic;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Forum;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Aplication.Services.Services;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using Talonario.Domain.Repositories;

namespace ProjetoFinal.Aplication.Services.Services.Forum;

public class ForumAppService : IForumAppService
{
    private readonly IForumThreadRepository _threadRepository;
    private readonly IForumPostRepository _postRepository;
    private readonly IAutomapApi _mapper;
    private readonly IUnityOfWork _unityOfWork;

    public ForumAppService(
        IForumThreadRepository threadRepository,
        IForumPostRepository postRepository,
        IUnityOfWork unityOfWork,
        IAutomapApi mapper)
    {
        _threadRepository = threadRepository;
        _postRepository = postRepository;
        _unityOfWork = unityOfWork;
        _mapper = mapper;
    }

    public async Task<ForumThreadDto> CreateThreadAsync(ForumThreadCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.MapFrom<ForumThread>(dto);
        entity.LastActivityAt = DateTime.UtcNow;
        var created = await _threadRepository.AddAsync(entity, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.MapFrom<ForumThreadDto>(created);
    }

    public async Task UpdateThreadAsync(Guid threadId, ForumThreadUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.FindAsync(threadId, cancellationToken);
        if (thread is null)
        {
            throw new BusinessException("Topico nao encontrado.", ECodigo.NaoEncontrado);
        }

        _mapper.MapTo(dto, thread);
        thread.UpdatedAt = DateTime.UtcNow;
        await _threadRepository.UpdateAsync(thread, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteThreadAsync(Guid threadId, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.FindAsync(threadId, cancellationToken);
        if (thread is null)
        {
            throw new BusinessException("Topico nao encontrado.", ECodigo.NaoEncontrado);
        }

        await _threadRepository.DeleteAsync(thread, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ForumThreadDto> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId, cancellationToken);
        if (thread is null)
        {
            throw new BusinessException("Topico nao encontrado.", ECodigo.NaoEncontrado);
        }

        return _mapper.MapFrom<ForumThreadDto>(thread);
    }

    public async Task<PagedResultDto<ForumThreadDto>> GetThreadsAsync(ForumThreadFilter filter, CancellationToken cancellationToken = default)
    {
        var result = await _threadRepository.GetAllAsync(filter, cancellationToken);
        return _mapper.MapFrom<PagedResultDto<ForumThreadDto>>(result);
    }

    public async Task<ForumPostDto> CreatePostAsync(ForumPostCreateDto dto, CancellationToken cancellationToken = default)
    {
        var thread = await _threadRepository.FindAsync(dto.ThreadId, cancellationToken);
        if (thread is null)
        {
            throw new BusinessException("Topico nao encontrado.", ECodigo.NaoEncontrado);
        }

        if (thread.IsLocked)
        {
            throw new BusinessException("Este topico esta bloqueado para novas mensagens.", ECodigo.NaoPermitido);
        }

        var post = _mapper.MapFrom<ForumPost>(dto);
        post.Attachments = _mapper.MapFrom<List<ForumPostAttachment>>(dto.Attachments);
        post.CreatedAt = DateTime.UtcNow;

        var created = await _postRepository.AddAsync(post, cancellationToken);
        thread.LastActivityAt = DateTime.UtcNow;
        await _threadRepository.UpdateAsync(thread, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.MapFrom<ForumPostDto>(created);
    }

    public async Task UpdatePostAsync(Guid postId, ForumPostUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.FindAsync(postId, cancellationToken);
        if (post is null)
        {
            throw new BusinessException("Mensagem nao encontrada.", ECodigo.NaoEncontrado);
        }

        _mapper.MapTo(dto, post);
        post.Attachments.Clear();
        foreach (var attachmentDto in dto.Attachments)
        {
            var attachment = _mapper.MapFrom<ForumPostAttachment>(attachmentDto);
            attachment.Id = Guid.NewGuid();
            attachment.ForumPostId = post.Id;
            post.Attachments.Add(attachment);
        }

        post.EditedAt = DateTime.UtcNow;
        await _postRepository.UpdateAsync(post, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.FindAsync(postId, cancellationToken);
        if (post is null)
        {
            throw new BusinessException("Mensagem nao encontrada.", ECodigo.NaoEncontrado);
        }

        await _postRepository.DeleteAsync(post, cancellationToken);
        await _unityOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResultDto<ForumPostDto>> GetPostsAsync(ForumPostFilter filter, CancellationToken cancellationToken = default)
    {
        var result = await _postRepository.GetAllAsync(filter, cancellationToken);
        return _mapper.MapFrom<PagedResultDto<ForumPostDto>>(result);
    }
}
