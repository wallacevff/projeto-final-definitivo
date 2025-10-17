using System;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Forum;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IForumAppService
{
    Task<ForumThreadDto> CreateThreadAsync(ForumThreadCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateThreadAsync(Guid threadId, ForumThreadUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteThreadAsync(Guid threadId, CancellationToken cancellationToken = default);
    Task<ForumThreadDto> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ForumThreadDto>> GetThreadsAsync(ForumThreadFilter filter, CancellationToken cancellationToken = default);

    Task<ForumPostDto> CreatePostAsync(ForumPostCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdatePostAsync(Guid postId, ForumPostUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeletePostAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ForumPostDto>> GetPostsAsync(ForumPostFilter filter, CancellationToken cancellationToken = default);
}
