using SubscriptIQ.Core.Common;
using SubscriptIQ.Core.ValueObjects;

namespace SubscriptIQ.Core.Entities;

/// <summary>
/// Represents a payment transaction
/// </summary>
public class Payment : Entity
{
    public Guid SubscriptionId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? StripePaymentIntentId { get; private set; }
    public string? StripeInvoiceId { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string IdempotencyKey { get; private set; }

    private Payment() { } // EF Core

    private Payment(Guid subscriptionId, Money amount, string idempotencyKey) : base()
    {
        SubscriptionId = subscriptionId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        IdempotencyKey = idempotencyKey;
    }

    public static Payment Create(Guid subscriptionId, Money amount, string? idempotencyKey = null)
    {
        idempotencyKey ??= Guid.NewGuid().ToString();
        return new Payment(subscriptionId, amount, idempotencyKey);
    }

    public void MarkAsSucceeded(string stripePaymentIntentId, string? stripeInvoiceId = null)
    {
        if (string.IsNullOrWhiteSpace(stripePaymentIntentId))
            throw new ArgumentException("Stripe payment intent ID is required", nameof(stripePaymentIntentId));

        Status = PaymentStatus.Succeeded;
        StripePaymentIntentId = stripePaymentIntentId;
        StripeInvoiceId = stripeInvoiceId;
        PaidAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void MarkAsFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason is required", nameof(reason));

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        FailedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void MarkAsRefunded()
    {
        if (Status != PaymentStatus.Succeeded)
            throw new InvalidOperationException("Only succeeded payments can be refunded");

        Status = PaymentStatus.Refunded;
        MarkAsUpdated();
    }

    public void MarkAsCancelled()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be cancelled");

        Status = PaymentStatus.Cancelled;
        MarkAsUpdated();
    }
}
