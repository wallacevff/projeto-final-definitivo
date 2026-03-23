using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;

namespace ProjetoFinal.Api.Hubs;

[Authorize]
public class ChatHub(
    IClassGroupRepository classGroupRepository,
    IClassEnrollmentRepository classEnrollmentRepository,
    ChatPresenceTracker presenceTracker) : Hub
{
    public async Task JoinClassGroup(string classGroupId)
    {
        if (!Guid.TryParse(classGroupId, out var parsedClassGroupId))
        {
            throw new BusinessException("Turma invalida para o chat.", ECodigo.MaRequisicao);
        }

        await EnsureUserCanAccessClassGroupAsync(parsedClassGroupId);

        var groupName = BuildClassGroupGroup(parsedClassGroupId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var users = presenceTracker.Register(parsedClassGroupId, ResolveCurrentUserId(), ResolveCurrentUserName(), Context.ConnectionId);
        await Clients.Group(groupName).SendAsync("PresenceSnapshot", users);
    }

    public async Task LeaveClassGroup(string classGroupId)
    {
        if (!Guid.TryParse(classGroupId, out var parsedClassGroupId))
        {
            return;
        }

        var groupName = BuildClassGroupGroup(parsedClassGroupId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        var snapshot = presenceTracker.Unregister(Context.ConnectionId);
        if (snapshot is not null)
        {
            await Clients.Group(groupName).SendAsync("PresenceSnapshot", snapshot.Users);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var snapshot = presenceTracker.Unregister(Context.ConnectionId);
        if (snapshot is not null)
        {
            await Clients.Group(BuildClassGroupGroup(snapshot.ClassGroupId)).SendAsync("PresenceSnapshot", snapshot.Users);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static string BuildClassGroupGroup(Guid classGroupId)
    {
        return $"chat-class-group-{classGroupId:N}";
    }

    private async Task EnsureUserCanAccessClassGroupAsync(Guid classGroupId)
    {
        var classGroup = await classGroupRepository.GetByIdAsync(classGroupId);
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

        var enrollment = await classEnrollmentRepository.FirstOrDefaultByPredicateAsync(
            item => item.ClassGroupId == classGroupId
                    && item.StudentId == userId
                    && item.Status == EnrollmentStatus.Approved);

        if (enrollment is null)
        {
            throw new BusinessException("Voce nao possui acesso a este chat.", ECodigo.NaoPermitido);
        }
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }

    private string ResolveCurrentUserName()
    {
        return Context.User?.FindFirstValue(ClaimTypes.Name) ?? "Usuario";
    }

    private bool IsInstructor()
    {
        return HasRole(UserRole.Instructor);
    }

    private bool IsAdministrator()
    {
        return HasRole(UserRole.Administrator);
    }

    private bool HasRole(UserRole expectedRole)
    {
        var roleValue = Context.User?.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrWhiteSpace(roleValue))
        {
            return false;
        }

        if (Enum.TryParse<UserRole>(roleValue, ignoreCase: true, out var enumRole))
        {
            return enumRole == expectedRole;
        }

        return int.TryParse(roleValue, out var numericRole) && numericRole == (int)expectedRole;
    }
}
