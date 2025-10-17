using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class SubmissionAttachmentMap : IContextEntityMap<SubmissionAttachment>
{
    public void Configure(EntityTypeBuilder<SubmissionAttachment> builder)
    {
        builder.ToTable("SubmissionAttachments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(p => p.IsVideo)
            .HasDefaultValue(false);

        builder.HasOne(p => p.Submission)
            .WithMany(p => p.Attachments)
            .HasForeignKey(p => p.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.MediaResource)
            .WithMany(p => p.SubmissionAttachments)
            .HasForeignKey(p => p.MediaResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
