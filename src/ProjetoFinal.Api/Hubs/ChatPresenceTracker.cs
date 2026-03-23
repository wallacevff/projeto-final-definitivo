using System.Collections.Concurrent;
using ProjetoFinal.Application.Contracts.Dto.Chat;

namespace ProjetoFinal.Api.Hubs;

public class ChatPresenceTracker
{
    private readonly ConcurrentDictionary<string, ConnectionRegistration> _connections = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, PresenceEntry>> _groupUsers = new();

    public IReadOnlyCollection<ChatPresenceUserDto> Register(Guid classGroupId, Guid userId, string userName, string connectionId)
    {
        var groupUsers = _groupUsers.GetOrAdd(classGroupId, _ => new ConcurrentDictionary<Guid, PresenceEntry>());
        var entry = groupUsers.GetOrAdd(userId, _ => new PresenceEntry(userName));
        entry.ConnectionIds[connectionId] = 0;
        _connections[connectionId] = new ConnectionRegistration(classGroupId, userId);
        return GetUsers(classGroupId);
    }

    public PresenceSnapshotResult? Unregister(string connectionId)
    {
        if (!_connections.TryRemove(connectionId, out var registration))
        {
            return null;
        }

        if (!_groupUsers.TryGetValue(registration.ClassGroupId, out var groupUsers))
        {
            return new PresenceSnapshotResult(registration.ClassGroupId, Array.Empty<ChatPresenceUserDto>());
        }

        if (groupUsers.TryGetValue(registration.UserId, out var entry))
        {
            entry.ConnectionIds.TryRemove(connectionId, out _);
            if (entry.ConnectionIds.IsEmpty)
            {
                groupUsers.TryRemove(registration.UserId, out _);
            }
        }

        if (groupUsers.IsEmpty)
        {
            _groupUsers.TryRemove(registration.ClassGroupId, out _);
            return new PresenceSnapshotResult(registration.ClassGroupId, Array.Empty<ChatPresenceUserDto>());
        }

        return new PresenceSnapshotResult(registration.ClassGroupId, GetUsers(registration.ClassGroupId));
    }

    public IReadOnlyCollection<ChatPresenceUserDto> GetUsers(Guid classGroupId)
    {
        if (!_groupUsers.TryGetValue(classGroupId, out var groupUsers))
        {
            return Array.Empty<ChatPresenceUserDto>();
        }

        return groupUsers
            .Select(item => new ChatPresenceUserDto
            {
                UserId = item.Key,
                UserName = item.Value.UserName
            })
            .OrderBy(item => item.UserName)
            .ToArray();
    }

    private sealed record ConnectionRegistration(Guid ClassGroupId, Guid UserId);

    public sealed record PresenceSnapshotResult(Guid ClassGroupId, IReadOnlyCollection<ChatPresenceUserDto> Users);

    private sealed class PresenceEntry
    {
        public PresenceEntry(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; }
        public ConcurrentDictionary<string, byte> ConnectionIds { get; } = new();
    }
}
