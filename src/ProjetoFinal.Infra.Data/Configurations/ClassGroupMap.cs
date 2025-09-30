using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ClassGroupMap : IContextEntityMap<ClassGroup>
{
    public void Configure(EntityTypeBuilder<ClassGroup> builder)
    {
        builder.ToTable("ClassGroups");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.EnrollmentCodeHash)
            .HasMaxLength(256);

        builder.Property(p => p.Capacity)
            .IsRequired();

        builder.Property(p => p.RequiresApproval)
            .HasDefaultValue(false);

        builder.Property(p => p.RequiresEnrollmentCode)
            .HasDefaultValue(false);

        builder.Property(p => p.EnableChat)
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(p => p.Course)
            .WithMany(p => p.ClassGroups)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Enrollments)
            .WithOne(p => p.ClassGroup)
            .HasForeignKey(p => p.ClassGroupId);

        builder.HasMany(p => p.ScopedContents)
            .WithOne(p => p.ClassGroup)
            .HasForeignKey(p => p.ClassGroupId);

        builder.HasMany(p => p.ActivityAudiences)
            .WithOne(p => p.ClassGroup)
            .HasForeignKey(p => p.ClassGroupId);

        builder.HasMany(p => p.ChatMessages)
            .WithOne(p => p.ClassGroup)
            .HasForeignKey(p => p.ClassGroupId);
    }
}
