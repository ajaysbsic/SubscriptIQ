using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Infrastructure.Configurations;

public class UsageTrackingConfiguration : IEntityTypeConfiguration<UsageTracking>
{
    public void Configure(EntityTypeBuilder<UsageTracking> builder)
    {
        builder.ToTable("UsageTrackings");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.TenantId)
            .IsRequired();
        
        builder.Property(u => u.SubscriptionId)
            .IsRequired();
        
        builder.Property(u => u.FeatureKey)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(u => u.CurrentUsage)
            .IsRequired();
        
        builder.Property(u => u.PeriodStart)
            .IsRequired();
        
        builder.Property(u => u.PeriodEnd)
            .IsRequired();
        
        builder.Property(u => u.CreatedAt)
            .IsRequired();
        
        builder.Property(u => u.UpdatedAt)
            .IsRequired();
        
        builder.HasIndex(u => new { u.SubscriptionId, u.FeatureKey, u.PeriodStart });
    }
}
