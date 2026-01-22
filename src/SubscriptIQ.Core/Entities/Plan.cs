using SubscriptIQ.Core.Common;
using SubscriptIQ.Core.ValueObjects;

namespace SubscriptIQ.Core.Entities;

/// <summary>
/// Represents a subscription plan with pricing and features
/// </summary>
public class Plan : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public BillingInterval BillingInterval { get; private set; }
    public bool IsActive { get; private set; }
    public int? TrialDays { get; private set; }
    public string? StripePriceId { get; private set; }
    
    // Features and limits
    private readonly List<string> _features = new();
    public IReadOnlyCollection<string> Features => _features.AsReadOnly();

    private Plan() { } // EF Core

    private Plan(string name, string description, Money price, BillingInterval billingInterval, int? trialDays = null) : base()
    {
        Name = name;
        Description = description;
        Price = price;
        BillingInterval = billingInterval;
        IsActive = true;
        TrialDays = trialDays;
    }

    public static Plan Create(string name, string description, Money price, BillingInterval billingInterval, int? trialDays = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Plan description is required", nameof(description));

        if (trialDays.HasValue && trialDays.Value < 0)
            throw new ArgumentException("Trial days cannot be negative", nameof(trialDays));

        return new Plan(name, description, price, billingInterval, trialDays);
    }

    public void AddFeature(string feature)
    {
        if (string.IsNullOrWhiteSpace(feature))
            throw new ArgumentException("Feature name is required", nameof(feature));

        if (!_features.Contains(feature))
        {
            _features.Add(feature);
            MarkAsUpdated();
        }
    }

    public void RemoveFeature(string feature)
    {
        if (_features.Remove(feature))
        {
            MarkAsUpdated();
        }
    }

    public void SetStripePriceId(string stripePriceId)
    {
        if (string.IsNullOrWhiteSpace(stripePriceId))
            throw new ArgumentException("Stripe price ID is required", nameof(stripePriceId));
        
        StripePriceId = stripePriceId;
        MarkAsUpdated();
    }

    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}
