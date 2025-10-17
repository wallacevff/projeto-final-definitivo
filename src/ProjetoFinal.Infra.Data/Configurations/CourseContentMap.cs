using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class CourseContentMap : IContextEntityMap<CourseContent>
{
    public void Configure(EntityTypeBuilder<CourseContent> builder)
    {
        builder.ToTable("CourseContents");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Summary)
            .HasMaxLength(500);

        builder.Property(p => p.ItemType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(p => p.IsDraft)
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(p => p.Course)
            .WithMany(p => p.Contents)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ClassGroup)
            .WithMany(p => p.ScopedContents)
            .HasForeignKey(p => p.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Attachments)
            .WithOne(p => p.CourseContent)
            .HasForeignKey(p => p.CourseContentId);
    }
}
