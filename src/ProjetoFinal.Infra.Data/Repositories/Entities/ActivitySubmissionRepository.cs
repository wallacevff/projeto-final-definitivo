using System;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Domain.Filters;
using ProjetoFinal.Domain.Repositories;
using ProjetoFinal.Infra.Data.Contexts;

namespace ProjetoFinal.Infra.Data.Repositories.Entities;

public class ActivitySubmissionRepository : DefaultRepository<ActivitySubmission, ActivitySubmissionFilter, Guid>, IActivitySubmissionRepository
{
    private readonly AppDbContext _context;

    public ActivitySubmissionRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<ActivitySubmission?> GetWithDetailsAsync(Guid submissionId, CancellationToken cancellationToken = default)
    {
        return _context.ActivitySubmissions
            .Include(submission => submission.Attachments)
                .ThenInclude(attachment => attachment.MediaResource)
            .Include(submission => submission.VideoAnnotations)
                .ThenInclude(annotation => annotation.Attachment)
                    .ThenInclude(attachment => attachment!.MediaResource)
            .Include(submission => submission.Activity)
            .Include(submission => submission.Student)
            .FirstOrDefaultAsync(submission => submission.Id == submissionId, cancellationToken);
    }
}
