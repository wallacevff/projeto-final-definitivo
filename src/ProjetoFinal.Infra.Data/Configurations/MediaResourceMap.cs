using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class MediaResourceMap : IContextEntityMap<MediaResource>
{
    public void Configure(EntityTypeBuilder<MediaResource> builder)
    {
        builder.ToTable("MediaResources");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.OriginalFileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(p => p.ContentType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.StoragePath)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(p => p.Kind)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Sha256)
            .HasMaxLength(128);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => p.Sha256).IsUnique().HasFilter("[Sha256] IS NOT NULL");
    }
}
