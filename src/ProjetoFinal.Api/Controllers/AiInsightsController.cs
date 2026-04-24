using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Ai;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;

namespace ProjetoFinal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ai-insights")]
[Route("api/v1/ai-insights")]
public class AiInsightsController : ControllerBase
{
    private readonly IAiInsightsAppService _service;

    public AiInsightsController(IAiInsightsAppService service)
    {
        _service = service;
    }

    [HttpGet("contents/{contentId:guid}/summary")]
    public Task<AiContentSummaryDto> GenerateContentSummaryAsync(
        [FromRoute] Guid contentId,
        CancellationToken cancellationToken = default)
    {
        return _service.GenerateContentSummaryAsync(contentId, cancellationToken);
    }

    [HttpGet("instructor/frequent-questions")]
    public Task<AiInstructorFrequentQuestionsDto> GetInstructorFrequentQuestionsAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            throw new BusinessException("Apenas instrutores podem consultar este painel.", ECodigo.NaoPermitido);
        }

        var instructorId = ResolveCurrentUserId();
        if (instructorId == Guid.Empty)
        {
            throw new BusinessException("Instrutor nao identificado.", ECodigo.NaoAutenticado);
        }

        return _service.GetInstructorFrequentQuestionsAsync(instructorId, cancellationToken);
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }

    private bool IsInstructor()
    {
        var roleValue = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(roleValue))
        {
            return false;
        }

        if (Enum.TryParse<UserRole>(roleValue, ignoreCase: true, out var role))
        {
            return role == UserRole.Instructor;
        }

        return int.TryParse(roleValue, out var numericRole) && numericRole == (int)UserRole.Instructor;
    }

    private bool IsAdministrator()
    {
        var roleValue = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(roleValue))
        {
            return false;
        }

        if (Enum.TryParse<UserRole>(roleValue, ignoreCase: true, out var role))
        {
            return role == UserRole.Administrator;
        }

        return int.TryParse(roleValue, out var numericRole) && numericRole == (int)UserRole.Administrator;
    }
}
