using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ForumPostMap : IContextEntityMap<ForumPost>
{
    public void Configure(EntityTypeBuilder<ForumPost> builder)
    {
        builder.ToTable("ForumPosts");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Message)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(p => p.Thread)
            .WithMany(p => p.Posts)
            .HasForeignKey(p => p.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ParentPost)
            .WithMany(p => p.Replies)
            .HasForeignKey(p => p.ParentPostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Attachments)
            .WithOne(p => p.ForumPost)
            .HasForeignKey(p => p.ForumPostId);
    }
}
