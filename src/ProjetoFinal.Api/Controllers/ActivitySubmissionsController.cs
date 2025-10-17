using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Activities;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[Route("api/activity-submissions")]
public class ActivitySubmissionsController : ControllerBase
{
    private readonly IActivitySubmissionAppService _service;

    public ActivitySubmissionsController(IActivitySubmissionAppService service)
    {
        _service = service;
    }

    [HttpPost]
    public Task<ActivitySubmissionDto> SubmitAsync(
        [FromBody] ActivitySubmissionCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        return _service.SubmitAsync(dto, cancellationToken);
    }

    [HttpPut("{submissionId:guid}")]
    public Task<ActivitySubmissionDto> UpdateAsync(
        [FromRoute] Guid submissionId,
        [FromBody] ActivitySubmissionUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        return _service.UpdateAsync(submissionId, dto, cancellationToken);
    }

    [HttpGet("{submissionId:guid}")]
    public Task<ActivitySubmissionDto> GetByIdAsync(
        [FromRoute] Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        return _service.GetByIdAsync(submissionId, cancellationToken);
    }

    [HttpGet]
    public Task<PagedResultDto<ActivitySubmissionDto>> GetAllAsync(
        [FromQuery] ActivitySubmissionFilter filter,
        CancellationToken cancellationToken = default)
    {
        return _service.GetAllAsync(filter, cancellationToken);
    }

    [HttpPost("annotations")]
    public Task<VideoAnnotationDto> AddAnnotationAsync(
        [FromBody] VideoAnnotationCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        return _service.AddAnnotationAsync(dto, cancellationToken);
    }

    [HttpPut("annotations/{annotationId:guid}")]
    public Task<VideoAnnotationDto> UpdateAnnotationAsync(
        [FromRoute] Guid annotationId,
        [FromBody] VideoAnnotationUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        return _service.UpdateAnnotationAsync(annotationId, dto, cancellationToken);
    }

    [HttpDelete("annotations/{annotationId:guid}")]
    public async Task<IActionResult> DeleteAnnotationAsync(
        [FromRoute] Guid annotationId,
        CancellationToken cancellationToken = default)
    {
        await _service.DeleteAnnotationAsync(annotationId, cancellationToken);
        return NoContent();
    }
}
