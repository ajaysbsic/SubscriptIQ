using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubscriptIQ.Core.Interfaces;
using SubscriptIQ.Infrastructure.BackgroundServices;
using SubscriptIQ.Infrastructure.Data;
using SubscriptIQ.Infrastructure.Repositories;
using SubscriptIQ.Infrastructure.Services;

namespace SubscriptIQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<SubscriptIQDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SubscriptIQDbContext>());

        // Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IEntitlementRepository, EntitlementRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUsageTrackingRepository, UsageTrackingRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Services
        services.AddScoped<StripePaymentService>();

        // Background Services
        services.AddHostedService<OutboxProcessorService>();

        return services;
    }
}
