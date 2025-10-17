using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class CourseSubscriptionMap : IContextEntityMap<CourseSubscription>
{
    public void Configure(EntityTypeBuilder<CourseSubscription> builder)
    {
        builder.ToTable("CourseSubscriptions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.SubscribedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => new { p.CourseId, p.StudentId }).IsUnique();

        builder.HasOne(p => p.Course)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Student)
            .WithMany(p => p.CourseSubscriptions)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
