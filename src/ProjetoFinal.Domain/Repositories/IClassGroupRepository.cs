using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IClassGroupRepository : IDefaultRepository<ClassGroup, ClassGroupFilter, Guid>
{
    Task<bool> HasAvailableSeatsAsync(Guid classGroupId, CancellationToken cancellationToken = default);
    Task<ClassGroupChatAccessInfo?> GetChatAccessInfoAsync(Guid classGroupId, CancellationToken cancellationToken = default);
}

public class ClassGroupChatAccessInfo
{
    public Guid Id { get; set; }
    public bool EnableChat { get; set; }
    public Guid CourseId { get; set; }
    public Guid InstructorId { get; set; }
}
