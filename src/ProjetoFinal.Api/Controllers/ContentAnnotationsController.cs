using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Contents;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;

namespace ProjetoFinal.Api.Controllers;

[Route("api/content-annotations")]
[Route("api/v1/content-annotations")]
public class ContentAnnotationsController : BaseController<
    ContentVideoAnnotationDto,
    ContentVideoAnnotationCreateDto,
    ContentVideoAnnotationUpdateDto,
    ContentVideoAnnotationFilter,
    Guid,
    IContentVideoAnnotationAppService>
{
    public ContentAnnotationsController(IContentVideoAnnotationAppService service) : base(service)
    {
    }

    [HttpPost]
    public override async Task<ContentVideoAnnotationDto> AddAsync(
        [FromBody] ContentVideoAnnotationCreateDto createDto,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveCurrentUserId();
        if (userId == Guid.Empty)
        {
            throw new BusinessException("Usuario nao identificado.", ECodigo.NaoAutenticado);
        }

        createDto.CreatedById = userId;
        return await base.AddAsync(createDto, cancellationToken);
    }

    [HttpGet]
    public override Task<PagedResultDto<ContentVideoAnnotationDto>> GetAllAsync(
        [FromQuery] ContentVideoAnnotationFilter filter,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveCurrentUserId();
        if (userId == Guid.Empty)
        {
            throw new BusinessException("Usuario nao identificado.", ECodigo.NaoAutenticado);
        }

        filter.CreatedById = userId;
        return base.GetAllAsync(filter, cancellationToken);
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }
}
