using System;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;

namespace ProjetoFinal.Domain.Repositories;

public interface IActivitySubmissionRepository : IDefaultRepository<ActivitySubmission, ActivitySubmissionFilter, Guid>
{
    Task<ActivitySubmission?> GetWithDetailsAsync(Guid submissionId, CancellationToken cancellationToken = default);
}
