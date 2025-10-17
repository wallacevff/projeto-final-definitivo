using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Media;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[Route("api/media-resources")]
public class MediaResourcesController : BaseController<
    MediaResourceDto,
    MediaResourceCreateDto,
    MediaResourceCreateDto,
    MediaResourceFilter,
    Guid,
    IMediaResourceAppService>
{
    public MediaResourcesController(IMediaResourceAppService service) : base(service)
    {
    }
}
