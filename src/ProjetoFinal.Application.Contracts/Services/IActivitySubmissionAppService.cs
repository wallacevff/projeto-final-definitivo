using System;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Activities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Application.Contracts.Services;

public interface IActivitySubmissionAppService
{
    Task<ActivitySubmissionDto> SubmitAsync(ActivitySubmissionCreateDto dto, CancellationToken cancellationToken = default);
    Task<ActivitySubmissionDto> UpdateAsync(Guid submissionId, ActivitySubmissionUpdateDto dto, CancellationToken cancellationToken = default);
    Task<ActivitySubmissionDto> GetByIdAsync(Guid submissionId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ActivitySubmissionDto>> GetAllAsync(ActivitySubmissionFilter filter, CancellationToken cancellationToken = default);
    Task<VideoAnnotationDto> AddAnnotationAsync(VideoAnnotationCreateDto dto, CancellationToken cancellationToken = default);
    Task<VideoAnnotationDto> UpdateAnnotationAsync(Guid annotationId, VideoAnnotationUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAnnotationAsync(Guid annotationId, CancellationToken cancellationToken = default);
}
