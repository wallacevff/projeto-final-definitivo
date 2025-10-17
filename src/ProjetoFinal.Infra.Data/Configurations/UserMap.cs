using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class UserMap : IContextEntityMap<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(p => p.Username)
            .HasMaxLength(80);

        builder.Property(p => p.PasswordHash)
            .HasMaxLength(180);

        builder.Property(p => p.Biography)
            .HasMaxLength(2000);

        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(512);

        builder.Property(p => p.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.HasIndex(p => p.Email).IsUnique();
        builder.HasIndex(p => p.ExternalId).IsUnique();
        builder.HasIndex(p => p.Username)
            .IsUnique()
            .HasFilter("[Username] IS NOT NULL");

    }
}
