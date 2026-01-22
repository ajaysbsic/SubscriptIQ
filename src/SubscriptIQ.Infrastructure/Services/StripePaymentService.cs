using Stripe;
using SubscriptIQ.Core.Common;
using SubscriptIQ.Core.Entities;
using SubscriptIQ.Core.Interfaces;
using SubscriptIQ.Core.ValueObjects;

namespace SubscriptIQ.Infrastructure.Services;

/// <summary>
/// Service for handling Stripe payment integration
/// </summary>
public class StripePaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CustomerService _customerService;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly Stripe.SubscriptionService _stripeSubscriptionService;

    public StripePaymentService(
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
        _customerService = new CustomerService();
        _paymentIntentService = new PaymentIntentService();
        _stripeSubscriptionService = new Stripe.SubscriptionService();
    }

    public async Task<Result<Customer>> CreateCustomerAsync(string email, string name, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = new Dictionary<string, string>
                {
                    { "tenant_id", tenantId ?? string.Empty }
                }
            };

            var customer = await _customerService.CreateAsync(options, cancellationToken: cancellationToken);
            return Result.Success(customer);
        }
        catch (StripeException ex)
        {
            return Result.Failure<Customer>($"Failed to create Stripe customer: {ex.Message}");
        }
    }

    public async Task<Result<PaymentIntent>> CreatePaymentIntentAsync(
        Guid subscriptionId, 
        Money amount, 
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for existing payment with same idempotency key
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existingPayment = await _paymentRepository.GetByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
                if (existingPayment != null && existingPayment.StripePaymentIntentId != null)
                {
                    var existingIntent = await _paymentIntentService.GetAsync(existingPayment.StripePaymentIntentId, cancellationToken: cancellationToken);
                    return Result.Success(existingIntent);
                }
            }

            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
                return Result.Failure<PaymentIntent>("Subscription not found");

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount.Amount * 100), // Convert to cents
                Currency = amount.Currency.ToLowerInvariant(),
                Metadata = new Dictionary<string, string>
                {
                    { "subscription_id", subscriptionId.ToString() },
                    { "tenant_id", subscription.TenantId.ToString() }
                }
            };

            // Create payment record
            var payment = Payment.Create(subscriptionId, amount, idempotencyKey);
            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var paymentIntent = await _paymentIntentService.CreateAsync(options, 
                new RequestOptions { IdempotencyKey = idempotencyKey }, 
                cancellationToken);

            payment.MarkAsSucceeded(paymentIntent.Id);
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(paymentIntent);
        }
        catch (StripeException ex)
        {
            return Result.Failure<PaymentIntent>($"Failed to create payment intent: {ex.Message}");
        }
    }

    public async Task<Result<Stripe.Subscription>> CreateStripeSubscriptionAsync(
        string customerId,
        string priceId,
        int? trialDays = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions { Price = priceId }
                }
            };

            if (trialDays.HasValue)
            {
                options.TrialPeriodDays = trialDays.Value;
            }

            var stripeSubscription = await _stripeSubscriptionService.CreateAsync(options, cancellationToken: cancellationToken);
            return Result.Success(stripeSubscription);
        }
        catch (StripeException ex)
        {
            return Result.Failure<Stripe.Subscription>($"Failed to create Stripe subscription: {ex.Message}");
        }
    }

    public async Task<Result> CancelStripeSubscriptionAsync(string stripeSubscriptionId, bool immediately = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (immediately)
            {
                await _stripeSubscriptionService.CancelAsync(stripeSubscriptionId, cancellationToken: cancellationToken);
            }
            else
            {
                var options = new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                };
                await _stripeSubscriptionService.UpdateAsync(stripeSubscriptionId, options, cancellationToken: cancellationToken);
            }

            return Result.Success();
        }
        catch (StripeException ex)
        {
            return Result.Failure($"Failed to cancel Stripe subscription: {ex.Message}");
        }
    }
}
