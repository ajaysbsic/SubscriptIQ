using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.ValueObjects;

namespace SubscriptIQ.Infrastructure.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();
            
            money.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });
        
        builder.Property(p => p.BillingInterval)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(p => p.IsActive)
            .IsRequired();
        
        builder.Property(p => p.TrialDays);
        
        builder.Property(p => p.StripePriceId)
            .HasMaxLength(100);
        
        builder.Property(p => p.CreatedAt)
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Store features as JSON
        builder.Property<string>("_featuresJson")
            .HasColumnName("Features")
            .HasColumnType("jsonb");
    }
}
