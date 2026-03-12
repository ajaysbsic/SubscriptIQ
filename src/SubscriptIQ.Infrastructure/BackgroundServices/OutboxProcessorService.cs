using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SubscriptIQ.Core.Interfaces;

namespace SubscriptIQ.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that processes outbox messages for reliable event publishing
/// </summary>
public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Outbox Processor Service is stopping");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var messages = await outboxRepository.GetUnprocessedAsync(batchSize: 50, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                _logger.LogInformation("Processing outbox message {MessageId} of type {EventType}", 
                    message.Id, message.EventType);

                // Here you would publish to your event bus (e.g., RabbitMQ, Azure Service Bus, etc.)
                // For now, we'll just mark as processed
                // In a real implementation, you'd deserialize the event and publish it
                
                message.MarkAsProcessed();
                await outboxRepository.UpdateAsync(message, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully processed outbox message {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                
                message.IncrementRetryCount(ex.Message);
                await outboxRepository.UpdateAsync(message, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
