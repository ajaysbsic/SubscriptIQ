using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Infrastructure.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.EventType)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(o => o.EventData)
            .IsRequired()
            .HasColumnType("jsonb");
        
        builder.Property(o => o.IsProcessed)
            .IsRequired();
        
        builder.Property(o => o.ProcessedAt);
        
        builder.Property(o => o.RetryCount)
            .IsRequired();
        
        builder.Property(o => o.ErrorMessage)
            .HasMaxLength(1000);
        
        builder.Property(o => o.CreatedAt)
            .IsRequired();
        
        builder.Property(o => o.UpdatedAt)
            .IsRequired();
        
        builder.HasIndex(o => new { o.IsProcessed, o.CreatedAt });
    }
}
