using System;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Enums;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ClassEnrollmentRepository : DefaultRepository<ClassEnrollment, ClassEnrollmentFilter, Guid>, IClassEnrollmentRepository
{
    private readonly AppDbContext _context;

    public ClassEnrollmentRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<bool> ExistsPendingRequestAsync(Guid classGroupId, Guid studentId, CancellationToken cancellationToken = default)
    {
        return _context.ClassEnrollments
            .AnyAsync(enrollment => enrollment.ClassGroupId == classGroupId
                                    && enrollment.StudentId == studentId
                                    && enrollment.Status == EnrollmentStatus.Pending,
                cancellationToken);
    }
}
