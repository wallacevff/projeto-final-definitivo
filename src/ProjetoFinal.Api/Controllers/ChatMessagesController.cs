using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProjetoFinal.Api.Hubs;
using ProjetoFinal.Application.Contracts.Dto;
using ProjetoFinal.Application.Contracts.Dto.Chat;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;

namespace ProjetoFinal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/chat/messages")]
[Route("api/v1/chat/messages")]
public class ChatMessagesController : ControllerBase
{
    private readonly IChatMessageAppService _service;
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly IClassEnrollmentRepository _classEnrollmentRepository;

    public ChatMessagesController(
        IChatMessageAppService service,
        IHubContext<ChatHub> chatHub,
        IClassGroupRepository classGroupRepository,
        IClassEnrollmentRepository classEnrollmentRepository)
    {
        _service = service;
        _chatHub = chatHub;
        _classGroupRepository = classGroupRepository;
        _classEnrollmentRepository = classEnrollmentRepository;
    }

    [HttpPost]
    public async Task<ChatMessageDto> SendAsync(
        [FromBody] ChatMessageCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        dto.SenderId = ResolveCurrentUserId();
        await EnsureUserCanAccessClassGroupAsync(dto.ClassGroupId, cancellationToken);

        var message = await _service.SendAsync(dto, cancellationToken);
        await _chatHub.Clients.Group(ChatHub.BuildClassGroupGroup(message.ClassGroupId))
            .SendAsync("MessageReceived", message, cancellationToken);

        return message;
    }

    [HttpPut("{messageId:guid}")]
    public async Task<ChatMessageDto> UpdateAsync(
        [FromRoute] Guid messageId,
        [FromBody] ChatMessageUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureUserOwnsMessageAsync(messageId, cancellationToken);

        var message = await _service.UpdateAsync(messageId, dto, cancellationToken);
        await _chatHub.Clients.Group(ChatHub.BuildClassGroupGroup(message.ClassGroupId))
            .SendAsync("MessageUpdated", message, cancellationToken);

        return message;
    }

    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken = default)
    {
        await EnsureUserOwnsMessageAsync(messageId, cancellationToken);
        var message = await _service.GetByIdAsync(messageId, cancellationToken);
        await _service.DeleteAsync(messageId, cancellationToken);

        await _chatHub.Clients.Group(ChatHub.BuildClassGroupGroup(message.ClassGroupId))
            .SendAsync("MessageDeleted", messageId, cancellationToken);

        return NoContent();
    }

    [HttpGet]
    public async Task<PagedResultDto<ChatMessageDto>> GetAllAsync(
        [FromQuery] ChatMessageFilter filter,
        CancellationToken cancellationToken = default)
    {
        if (filter.ClassGroupId is null || filter.ClassGroupId == Guid.Empty)
        {
            throw new BusinessException("Informe a turma do chat.", ECodigo.MaRequisicao);
        }

        await EnsureUserCanAccessClassGroupAsync(filter.ClassGroupId.Value, cancellationToken);
        return await _service.GetMessagesAsync(filter, cancellationToken);
    }

    private async Task EnsureUserOwnsMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var userId = ResolveCurrentUserId();
        if (userId == Guid.Empty)
        {
            throw new BusinessException("Usuario nao identificado.", ECodigo.NaoAutenticado);
        }

        if (IsAdministrator())
        {
            return;
        }

        var message = await _service.GetByIdAsync(messageId, cancellationToken);
        if (message.SenderId != userId)
        {
            throw new BusinessException("Voce nao pode alterar mensagens de outro usuario.", ECodigo.NaoPermitido);
        }
    }

    private async Task EnsureUserCanAccessClassGroupAsync(Guid classGroupId, CancellationToken cancellationToken)
    {
        var classGroup = await _classGroupRepository.GetByIdAsync(classGroupId, cancellationToken);
        if (classGroup is null)
        {
            throw new BusinessException("Turma nao encontrada.", ECodigo.NaoEncontrado);
        }

        if (!classGroup.EnableChat)
        {
            throw new BusinessException("O chat desta turma esta desabilitado.", ECodigo.NaoPermitido);
        }

        if (IsAdministrator())
        {
            return;
        }

        var userId = ResolveCurrentUserId();
        if (userId == Guid.Empty)
        {
            throw new BusinessException("Usuario nao identificado.", ECodigo.NaoAutenticado);
        }

        if (IsInstructor() && classGroup.Course?.InstructorId == userId)
        {
            return;
        }

        var enrollment = await _classEnrollmentRepository.FirstOrDefaultByPredicateAsync(
            item => item.ClassGroupId == classGroupId
                    && item.StudentId == userId
                    && item.Status == EnrollmentStatus.Approved,
            cancellationToken);

        if (enrollment is null)
        {
            throw new BusinessException("Voce nao possui acesso a este chat.", ECodigo.NaoPermitido);
        }
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }

    private bool IsInstructor()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return string.Equals(role, nameof(UserRole.Instructor), StringComparison.OrdinalIgnoreCase);
    }

    private bool IsAdministrator()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return string.Equals(role, nameof(UserRole.Administrator), StringComparison.OrdinalIgnoreCase);
    }
}
