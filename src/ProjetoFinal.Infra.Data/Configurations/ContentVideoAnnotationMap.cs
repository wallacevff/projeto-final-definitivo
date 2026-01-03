using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ContentVideoAnnotationMap : IContextEntityMap<ContentVideoAnnotation>
{
    public void Configure(EntityTypeBuilder<ContentVideoAnnotation> builder)
    {
        builder.ToTable("ContentVideoAnnotations");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.TimeMarkerSeconds)
            .IsRequired();

        builder.HasOne(p => p.ContentAttachment)
            .WithMany()
            .HasForeignKey(p => p.ContentAttachmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
