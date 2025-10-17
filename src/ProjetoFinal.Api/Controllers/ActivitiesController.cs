using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Activities;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[Route("api/activities")]
public class ActivitiesController : BaseController<
    ActivityDto,
    ActivityCreateDto,
    ActivityUpdateDto,
    ActivityFilter,
    Guid,
    IActivityAppService>
{
    public ActivitiesController(IActivityAppService service) : base(service)
    {
    }
}
