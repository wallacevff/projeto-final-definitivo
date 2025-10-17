using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IClassEnrollmentRepository : IDefaultRepository<ClassEnrollment, ClassEnrollmentFilter, Guid>
{
    Task<bool> ExistsPendingRequestAsync(Guid classGroupId, Guid studentId, CancellationToken cancellationToken = default);
}
