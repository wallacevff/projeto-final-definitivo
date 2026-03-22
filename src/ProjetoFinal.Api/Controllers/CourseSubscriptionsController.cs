using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Courses;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
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
    private readonly ICourseSubscriptionRepository _subscriptionRepository;

    public CourseSubscriptionsController(
        ICourseSubscriptionAppService service,
        ICourseSubscriptionRepository subscriptionRepository) : base(service)
    {
        _subscriptionRepository = subscriptionRepository;
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

    [HttpDelete("{id:guid}")]
    public override async Task<CourseSubscriptionDto> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var studentId = ResolveCurrentUserId();
        if (studentId == Guid.Empty)
        {
            throw new BusinessException("Aluno nao identificado.", ECodigo.NaoAutenticado);
        }

        var subscription = await _subscriptionRepository.FindAsync(id, cancellationToken);
        if (subscription is null)
        {
            throw new BusinessException("Inscricao no curso nao encontrada.", ECodigo.NaoEncontrado);
        }

        if (subscription.StudentId != studentId)
        {
            throw new BusinessException("Voce nao pode remover a inscricao de outro aluno.", ECodigo.NaoPermitido);
        }

        return await base.DeleteAsync(id, cancellationToken);
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }
}
