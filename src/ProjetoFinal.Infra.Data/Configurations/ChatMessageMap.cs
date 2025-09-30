using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjetoFinal.Domain.Entities;
using ProjetoFinal.Infra.Data.Interfaces;

namespace ProjetoFinal.Infra.Data.Configurations;

public class ChatMessageMap : IContextEntityMap<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.SentAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.IsSystemMessage)
            .HasDefaultValue(false);

        builder.HasOne(p => p.ClassGroup)
            .WithMany(p => p.ChatMessages)
            .HasForeignKey(p => p.ClassGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Sender)
            .WithMany()
            .HasForeignKey(p => p.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ReplyTo)
            .WithMany(p => p.Replies)
            .HasForeignKey(p => p.ReplyToMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.MediaResource)
            .WithMany()
            .HasForeignKey(p => p.MediaResourceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
