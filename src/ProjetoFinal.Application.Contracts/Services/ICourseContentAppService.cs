using System;
using ProjetoFinal.Application.Contracts.Dto.Contents;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface ICourseContentAppService : IDefaultService<CourseContentDto, CourseContentCreateDto, CourseContentUpdateDto, CourseContentFilter, Guid>
{
    Task PublishAsync(Guid contentId, CancellationToken cancellationToken = default);
    Task UpdateDisplayOrderAsync(Guid contentId, int displayOrder, CancellationToken cancellationToken = default);
}
