using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class VideoAnnotationMap : IContextEntityMap<VideoAnnotation>
{
    public void Configure(EntityTypeBuilder<VideoAnnotation> builder)
    {
        builder.ToTable("VideoAnnotations");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.TimeMarkerSeconds)
            .IsRequired();

        builder.HasOne(p => p.Submission)
            .WithMany(p => p.VideoAnnotations)
            .HasForeignKey(p => p.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Attachment)
            .WithMany()
            .HasForeignKey(p => p.AttachmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
