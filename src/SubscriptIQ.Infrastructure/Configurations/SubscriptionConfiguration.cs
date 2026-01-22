using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.ValueObjects;

namespace SubscriptIQ.Infrastructure.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.TenantId)
            .IsRequired();
        
        builder.Property(s => s.PlanId)
            .IsRequired();
        
        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(s => s.StartDate)
            .IsRequired();
        
        builder.Property(s => s.EndDate);
        
        builder.Property(s => s.TrialEndDate);
        
        builder.Property(s => s.CancelledAt);
        
        builder.Property(s => s.CurrentPeriodStart)
            .IsRequired();
        
        builder.Property(s => s.CurrentPeriodEnd)
            .IsRequired();
        
        builder.Property(s => s.StripeSubscriptionId)
            .HasMaxLength(100);
        
        builder.Property(s => s.PendingDowngradePlanId);
        
        builder.Property(s => s.Version)
            .IsRequired()
            .IsConcurrencyToken();
        
        builder.Property(s => s.CreatedAt)
            .IsRequired();
        
        builder.Property(s => s.UpdatedAt)
            .IsRequired();
        
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => new { s.TenantId, s.Status });
    }
}
