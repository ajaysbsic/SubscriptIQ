using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Infrastructure.Configurations;

public class EntitlementConfiguration : IEntityTypeConfiguration<Entitlement>
{
    public void Configure(EntityTypeBuilder<Entitlement> builder)
    {
        builder.ToTable("Entitlements");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.PlanId)
            .IsRequired();
        
        builder.Property(e => e.FeatureKey)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(e => e.IsEnabled)
            .IsRequired();
        
        builder.Property(e => e.Limit);
        
        builder.Property(e => e.Metadata)
            .HasColumnType("jsonb");
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.UpdatedAt)
            .IsRequired();
        
        builder.HasIndex(e => new { e.PlanId, e.FeatureKey })
            .IsUnique();
    }
}
