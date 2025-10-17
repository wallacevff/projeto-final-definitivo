using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ActivityAttachmentMap : IContextEntityMap<ActivityAttachment>
{
    public void Configure(EntityTypeBuilder<ActivityAttachment> builder)
    {
        builder.ToTable("ActivityAttachments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Caption)
            .HasMaxLength(300);

        builder.HasOne(p => p.Activity)
            .WithMany(p => p.Attachments)
            .HasForeignKey(p => p.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.MediaResource)
            .WithMany(p => p.ActivityAttachments)
            .HasForeignKey(p => p.MediaResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
