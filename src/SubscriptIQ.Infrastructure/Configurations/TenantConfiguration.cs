using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptIQ.Core.Entities;

namespace SubscriptIQ.Infrastructure.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(t => t.ApiKey)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(t => t.ApiKey)
            .IsUnique();
        
        builder.Property(t => t.StripeCustomerId)
            .HasMaxLength(100);
        
        builder.Property(t => t.IsActive)
            .IsRequired();
        
        builder.Property(t => t.CreatedAt)
            .IsRequired();
        
        builder.Property(t => t.UpdatedAt)
            .IsRequired();
    }
}
