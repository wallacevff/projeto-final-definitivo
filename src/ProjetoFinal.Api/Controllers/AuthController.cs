using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Application.Contracts.Dto.Auth;
using ProjetoFinal.Application.Contracts.Services;

namespace ProjetoFinal.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;

    public AuthController(IAuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> LoginAsync(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _authAppService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }
}
