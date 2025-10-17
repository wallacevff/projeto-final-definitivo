using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Contents;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[Route("api/course-contents")]
public class CourseContentsController : BaseController<
    CourseContentDto,
    CourseContentCreateDto,
    CourseContentUpdateDto,
    CourseContentFilter,
    Guid,
    ICourseContentAppService>
{
    public CourseContentsController(ICourseContentAppService service) : base(service)
    {
    }

    [HttpPut("{contentId:guid}/publish")]
    public async Task<IActionResult> PublishAsync(
        [FromRoute] Guid contentId,
        CancellationToken cancellationToken = default)
    {
        await Service.PublishAsync(contentId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{contentId:guid}/display-order")]
    public async Task<IActionResult> UpdateDisplayOrderAsync(
        [FromRoute] Guid contentId,
        [FromBody] UpdateDisplayOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        await Service.UpdateDisplayOrderAsync(contentId, request.DisplayOrder, cancellationToken);
        return NoContent();
    }

    public record UpdateDisplayOrderRequest(int DisplayOrder);
}
