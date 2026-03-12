using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace SubscriptIQ.Api.Endpoints;

public static class WebhookEndpoints
{
    public static void MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/webhooks/stripe", async (
            HttpRequest request,
            [FromServices] IConfiguration configuration,
            [FromServices] ILogger<Program> logger) =>
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();
            var webhookSecret = configuration["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    request.Headers["Stripe-Signature"],
                    webhookSecret,
                    throwOnApiVersionMismatch: false
                );

                logger.LogInformation("Stripe webhook received: {EventType}", stripeEvent.Type);

                // Handle different event types
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        logger.LogInformation("Payment succeeded: {PaymentIntentId}", paymentIntent?.Id);
                        // Handle successful payment
                        break;

                    case "payment_intent.payment_failed":
                        var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                        logger.LogWarning("Payment failed: {PaymentIntentId}", failedPayment?.Id);
                        // Handle failed payment
                        break;

                    case "customer.subscription.created":
                    case "customer.subscription.updated":
                        var subscription = stripeEvent.Data.Object as Subscription;
                        logger.LogInformation("Subscription updated: {SubscriptionId}", subscription?.Id);
                        // Handle subscription changes
                        break;

                    case "customer.subscription.deleted":
                        var deletedSubscription = stripeEvent.Data.Object as Subscription;
                        logger.LogInformation("Subscription cancelled: {SubscriptionId}", deletedSubscription?.Id);
                        // Handle subscription cancellation
                        break;

                    case "invoice.payment_succeeded":
                        var invoice = stripeEvent.Data.Object as Invoice;
                        logger.LogInformation("Invoice paid: {InvoiceId}", invoice?.Id);
                        // Handle invoice payment
                        break;

                    case "invoice.payment_failed":
                        var failedInvoice = stripeEvent.Data.Object as Invoice;
                        logger.LogWarning("Invoice payment failed: {InvoiceId}", failedInvoice?.Id);
                        // Handle failed invoice
                        break;

                    default:
                        logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Results.Ok();
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Stripe webhook error");
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("StripeWebhook")
        .WithTags("Webhooks")
        .WithOpenApi()
        .DisableAntiforgery(); // Webhooks don't use antiforgery tokens
    }
}
