using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Users;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Api.Controllers;

[Route("api/users")]
public class UsersController : BaseController<
    UserDto,
    UserCreateDto,
    UserUpdateDto,
    UserFilter,
    Guid,
    IUserAppService>
{
    public UsersController(IUserAppService service) : base(service)
    {
    }

    [HttpGet("by-email")]
    public async Task<ActionResult<UserDto>> GetByEmailAsync(
        [FromQuery] string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("O parametro 'email' e obrigatorio.");
        }

        var user = await Service.GetByEmailAsync(email, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
