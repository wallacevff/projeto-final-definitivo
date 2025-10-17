using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ContentAttachmentMap : IContextEntityMap<ContentAttachment>
{
    public void Configure(EntityTypeBuilder<ContentAttachment> builder)
    {
        builder.ToTable("ContentAttachments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Caption)
            .HasMaxLength(300);

        builder.Property(p => p.IsPrimary)
            .HasDefaultValue(false);

        builder.HasOne(p => p.CourseContent)
            .WithMany(p => p.Attachments)
            .HasForeignKey(p => p.CourseContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.MediaResource)
            .WithMany(p => p.ContentAttachments)
            .HasForeignKey(p => p.MediaResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
