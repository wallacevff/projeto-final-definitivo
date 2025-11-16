using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.ClassGroups;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjetoFinal.Api.Controllers;

[Route("api/class-groups")]
[Route("api/v1/class-groups")]
public class ClassGroupsController : BaseController<
    ClassGroupDto,
    ClassGroupCreateDto,
    ClassGroupUpdateDto,
    ClassGroupFilter,
    Guid,
    IClassGroupAppService>
{
    public ClassGroupsController(IClassGroupAppService service) : base(service)
    {
    }

    [HttpPost("{classGroupId:guid}/enrollments")]
    public async Task<ActionResult<ClassEnrollmentDto>> RequestEnrollmentAsync(
        [FromRoute] Guid classGroupId,
        [FromBody] ClassEnrollmentRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto is null)
        {
            return BadRequest("Dados de inscricao nao informados.");
        }

        if (dto.ClassGroupId == Guid.Empty)
        {
            dto.ClassGroupId = classGroupId;
        }
        else if (dto.ClassGroupId != classGroupId)
        {
            return BadRequest("ClassGroupId divergente do valor na rota.");
        }

        var studentId = ResolveCurrentUserId();
        if (studentId == Guid.Empty)
        {
            throw new BusinessException("Aluno nao identificado.", ECodigo.NaoAutenticado);
        }

        dto.StudentId = studentId;

        var enrollment = await Service.RequestEnrollmentAsync(dto, cancellationToken);
        return Ok(enrollment);
    }

    [HttpPost("enrollments/decision")]
    public Task<ClassEnrollmentDto> DecideEnrollmentAsync(
        [FromBody] ClassEnrollmentDecisionDto dto,
        CancellationToken cancellationToken = default)
    {
        return Service.DecideEnrollmentAsync(dto, cancellationToken);
    }

    [HttpDelete("enrollments/{enrollmentId:guid}")]
    public async Task<IActionResult> RemoveEnrollmentAsync(
        [FromRoute] Guid enrollmentId,
        CancellationToken cancellationToken = default)
    {
        await Service.RemoveEnrollmentAsync(enrollmentId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{classGroupId:guid}/availability")]
    public async Task<ActionResult<AvailabilityResponse>> HasAvailableSeatsAsync(
        [FromRoute] Guid classGroupId,
        CancellationToken cancellationToken = default)
    {
        var hasSeats = await Service.HasAvailableSeatsAsync(classGroupId, cancellationToken);
        return Ok(new AvailabilityResponse(hasSeats));
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }

    public record AvailabilityResponse(bool HasSeats);
}
