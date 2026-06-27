// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging
//  Layer:          Infrastructure
//  File:           ServiceBusEventPublisher.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    IIntegrationEventPublisher backed by MassTransit and Azure Service Bus.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Publishers;

/// <summary>
/// <see cref="IIntegrationEventPublisher"/> backed by MassTransit and Azure Service Bus.
/// Wraps <see cref="IBus.Publish{T}"/> to enforce canonical serialisation and logging.
/// </summary>
public sealed class ServiceBusEventPublisher(IBus bus, ILogger<ServiceBusEventPublisher> logger)
    : IIntegrationEventPublisher
{
    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        try
        {
            await bus.Publish(integrationEvent, cancellationToken);
            logger.LogInformation("Published integration event {EventType}", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish integration event {EventType}", typeof(TEvent).Name);
            throw new MessagingException(typeof(TEvent).Name, string.Empty, ex);
        }
    }
}
