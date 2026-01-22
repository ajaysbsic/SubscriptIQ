using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.ValueObjects;

namespace SubscriptIQ.Infrastructure.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.SubscriptionId)
            .IsRequired();
        
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();
            
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });
        
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(p => p.StripePaymentIntentId)
            .HasMaxLength(100);
        
        builder.Property(p => p.StripeInvoiceId)
            .HasMaxLength(100);
        
        builder.Property(p => p.PaidAt);
        
        builder.Property(p => p.FailedAt);
        
        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);
        
        builder.Property(p => p.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(p => p.CreatedAt)
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .IsRequired();
        
        builder.HasIndex(p => p.IdempotencyKey)
            .IsUnique();
        
        builder.HasIndex(p => p.SubscriptionId);
    }
}
