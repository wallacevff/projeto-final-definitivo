using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class CourseMap : IContextEntityMap<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(220);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(300);

        builder.Property(p => p.DetailedDescription)
            .HasMaxLength(4000);

        builder.Property(p => p.EnrollmentInstructions)
            .HasMaxLength(1000);

        builder.Property(p => p.CategoryName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Mode)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.EnableForum)
            .HasDefaultValue(true);

        builder.Property(p => p.EnableChat)
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => p.Slug).IsUnique();

        builder.HasOne(p => p.Instructor)
            .WithMany(p => p.CoursesAsInstructor)
            .HasForeignKey(p => p.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ThumbnailMedia)
            .WithMany()
            .HasForeignKey(p => p.ThumbnailMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.ClassGroups)
            .WithOne(p => p.Course)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Contents)
            .WithOne(p => p.Course)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.ForumThreads)
            .WithOne(p => p.Course)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Activities)
            .WithOne(p => p.Course)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Subscriptions)
            .WithOne(p => p.Course)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
