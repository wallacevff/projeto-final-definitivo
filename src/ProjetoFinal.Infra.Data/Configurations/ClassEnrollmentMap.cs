using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ClassEnrollmentMap : IContextEntityMap<ClassEnrollment>
{
    public void Configure(EntityTypeBuilder<ClassEnrollment> builder)
    {
        builder.ToTable("ClassEnrollments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.DecisionReason)
            .HasMaxLength(500);

        builder.Property(p => p.RequestedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => new { p.ClassGroupId, p.StudentId }).IsUnique();

        builder.HasOne(p => p.ClassGroup)
            .WithMany(p => p.Enrollments)
            .HasForeignKey(p => p.ClassGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Student)
            .WithMany(p => p.ClassEnrollments)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.DecidedBy)
            .WithMany()
            .HasForeignKey(p => p.DecidedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
