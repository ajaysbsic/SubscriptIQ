using SubscriptIQ.Core.Common;

namespace SubscriptIQ.Core.Entities;

/// <summary>
/// Represents a tenant in the multi-tenant system
/// </summary>
public class Tenant : Entity
{
    public string Name { get; private set; }
    public string ApiKey { get; private set; }
    public bool IsActive { get; private set; }
    public string? StripeCustomerId { get; private set; }

    private Tenant() { } // EF Core

    private Tenant(string name, string apiKey) : base()
    {
        Name = name;
        ApiKey = apiKey;
        IsActive = true;
    }

    public static Tenant Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required", nameof(name));

        var apiKey = GenerateApiKey();
        return new Tenant(name, apiKey);
    }

    public void SetStripeCustomerId(string stripeCustomerId)
    {
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            throw new ArgumentException("Stripe customer ID is required", nameof(stripeCustomerId));
        
        StripeCustomerId = stripeCustomerId;
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

    private static string GenerateApiKey()
    {
        return $"sk_{Guid.NewGuid():N}_{Guid.NewGuid():N}";
    }
}
