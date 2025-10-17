using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ActivitySubmissionMap : IContextEntityMap<ActivitySubmission>
{
    public void Configure(EntityTypeBuilder<ActivitySubmission> builder)
    {
        builder.ToTable("ActivitySubmissions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Score)
            .HasPrecision(10, 2);

        builder.Property(p => p.Feedback)
            .HasMaxLength(2000);

        builder.Property(p => p.TextAnswer)
            .HasMaxLength(4000);

        builder.Property(p => p.SubmittedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => new { p.ActivityId, p.StudentId }).IsUnique();

        builder.HasOne(p => p.Activity)
            .WithMany(p => p.Submissions)
            .HasForeignKey(p => p.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ClassGroup)
            .WithMany()
            .HasForeignKey(p => p.ClassGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Student)
            .WithMany()
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.GradedBy)
            .WithMany()
            .HasForeignKey(p => p.GradedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Attachments)
            .WithOne(p => p.Submission)
            .HasForeignKey(p => p.SubmissionId);

        builder.HasMany(p => p.VideoAnnotations)
            .WithOne(p => p.Submission)
            .HasForeignKey(p => p.SubmissionId);
    }
}
