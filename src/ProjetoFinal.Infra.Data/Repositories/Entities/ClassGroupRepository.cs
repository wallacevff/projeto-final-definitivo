using System;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ClassGroupRepository : DefaultRepository<ClassGroup, ClassGroupFilter, Guid>, IClassGroupRepository
{
    private readonly AppDbContext _context;

    public ClassGroupRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> HasAvailableSeatsAsync(Guid classGroupId, CancellationToken cancellationToken = default)
    {
        var groupInfo = await _context.ClassGroups
            .Where(group => group.Id == classGroupId)
            .Select(group => new
            {
                group.Capacity,
                Approved = group.Enrollments
                    .Count(enrollment => enrollment.Status == EnrollmentStatus.Approved)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (groupInfo is null)
        {
            return false;
        }

        if (groupInfo.Capacity <= 0)
        {
            return true;
        }

        return groupInfo.Approved < groupInfo.Capacity;
    }
}
