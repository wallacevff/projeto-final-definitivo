using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjetoFinal.Application.Contracts.Services;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Shared.Enums;
using ProjetoFinal.Domain.Shared.Exceptions;

namespace ProjetoFinal.Api.Hubs;

[Authorize]
public class ForumHub(ICourseAppService courseService, IForumAppService forumService) : Hub
{
    public async Task JoinThread(string threadId)
    {
        if (!Guid.TryParse(threadId, out var parsedThreadId))
        {
            throw new BusinessException("Topico invalido para conexao em tempo real.", ECodigo.MaRequisicao);
        }

        var thread = await forumService.GetThreadByIdAsync(parsedThreadId);
        await EnsureInstructorOwnsCourseAsync(thread.CourseId);
        await Groups.AddToGroupAsync(Context.ConnectionId, BuildThreadGroup(parsedThreadId));
    }

    public Task LeaveThread(string threadId)
    {
        if (!Guid.TryParse(threadId, out var parsedThreadId))
        {
            return Task.CompletedTask;
        }

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildThreadGroup(parsedThreadId));
    }

    public static string BuildThreadGroup(Guid threadId)
    {
        return $"forum-thread-{threadId:N}";
    }

    private Guid ResolveCurrentUserId()
    {
        var identifier = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(identifier, out var id) ? id : Guid.Empty;
    }

    private bool IsInstructor()
    {
        var roleValue = Context.User?.FindFirstValue(ClaimTypes.Role);
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
        var roleValue = Context.User?.FindFirstValue(ClaimTypes.Role);
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

    private async Task EnsureInstructorOwnsCourseAsync(Guid courseId)
    {
        if (!IsInstructor() || IsAdministrator())
        {
            return;
        }

        var instructorId = ResolveCurrentUserId();
        if (instructorId == Guid.Empty)
        {
            throw new BusinessException("Instrutor nao identificado.", ECodigo.NaoAutenticado);
        }

        var course = await courseService.GetByIdAsync(courseId);
        if (course.InstructorId != instructorId)
        {
            throw new BusinessException("Acesso nao permitido ao topico.", ECodigo.NaoPermitido);
        }
    }
}
