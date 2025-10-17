using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ActivityAudienceMap : IContextEntityMap<ActivityAudience>
{
    public void Configure(EntityTypeBuilder<ActivityAudience> builder)
    {
        builder.ToTable("ActivityAudiences");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.HasIndex(p => new { p.ActivityId, p.ClassGroupId }).IsUnique();

        builder.HasOne(p => p.Activity)
            .WithMany(p => p.Audiences)
            .HasForeignKey(p => p.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ClassGroup)
            .WithMany(p => p.ActivityAudiences)
            .HasForeignKey(p => p.ClassGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
