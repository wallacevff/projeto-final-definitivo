using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjetoFinal.Api.Controllers;

[Route("api/course-subscriptions")]
[Route("api/v1/course-subscriptions")]
public class CourseSubscriptionsController : BaseController<
    CourseSubscriptionDto,
    CourseSubscriptionCreateDto,
    CourseSubscriptionCreateDto,
    CourseSubscriptionFilter,
    Guid,
    ICourseSubscriptionAppService>
{
    public CourseSubscriptionsController(ICourseSubscriptionAppService service) : base(service)
    {
    }

    [HttpPost]
    public override async Task<CourseSubscriptionDto> AddAsync(
        [FromBody] CourseSubscriptionCreateDto createDto,
        CancellationToken cancellationToken = default)
    {
        var studentId = ResolveCurrentUserId();
        if (studentId == Guid.Empty)
        {
            throw new BusinessException("Aluno nao identificado.", ECodigo.NaoAutenticado);
        }

        createDto.StudentId = studentId;
        return await base.AddAsync(createDto, cancellationToken);
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }
}
