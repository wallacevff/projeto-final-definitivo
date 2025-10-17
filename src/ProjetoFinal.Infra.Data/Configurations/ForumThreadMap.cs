using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ForumThreadMap : IContextEntityMap<ForumThread>
{
    public void Configure(EntityTypeBuilder<ForumThread> builder)
    {
        builder.ToTable("ForumThreads");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.IsLocked)
            .HasDefaultValue(false);

        builder.Property(p => p.IsPinned)
            .HasDefaultValue(false);

        builder.Property(p => p.LastActivityAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => new { p.CourseId, p.ClassGroupId });

        builder.Property(p => p.ClassGroupId)
            .IsRequired();

        builder.HasOne(p => p.Course)
            .WithMany(p => p.ForumThreads)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ClassGroup)
            .WithMany(p => p.ForumThreads)
            .HasForeignKey(p => p.ClassGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Posts)
            .WithOne(p => p.Thread)
            .HasForeignKey(p => p.ThreadId);
    }
}
