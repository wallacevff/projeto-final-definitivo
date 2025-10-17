using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ActivityMap : IContextEntityMap<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.Scope)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.MaxScore)
            .HasPrecision(10, 2);

        builder.Property(p => p.AllowLateSubmissions)
            .HasDefaultValue(false);

        builder.Property(p => p.VisibleToStudents)
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.ClassGroupId)
            .IsRequired();

        builder.HasOne(p => p.Course)
            .WithMany(p => p.Activities)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ClassGroup)
            .WithMany(p => p.Activities)
            .HasForeignKey(p => p.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ReferenceContent)
            .WithMany()
            .HasForeignKey(p => p.ReferenceContentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Audiences)
            .WithOne(p => p.Activity)
            .HasForeignKey(p => p.ActivityId);

        builder.HasMany(p => p.Attachments)
            .WithOne(p => p.Activity)
            .HasForeignKey(p => p.ActivityId);

        builder.HasMany(p => p.Submissions)
            .WithOne(p => p.Activity)
            .HasForeignKey(p => p.ActivityId);
    }
}
