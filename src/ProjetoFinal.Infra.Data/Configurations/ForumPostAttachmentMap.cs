using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ForumPostAttachmentMap : IContextEntityMap<ForumPostAttachment>
{
    public void Configure(EntityTypeBuilder<ForumPostAttachment> builder)
    {
        builder.ToTable("ForumPostAttachments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Caption)
            .HasMaxLength(300);

        builder.HasOne(p => p.ForumPost)
            .WithMany(p => p.Attachments)
            .HasForeignKey(p => p.ForumPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.MediaResource)
            .WithMany(p => p.ForumPostAttachments)
            .HasForeignKey(p => p.MediaResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
