// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging
//  Layer:          Infrastructure
//  File:           IIntegrationEventPublisher.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstraction over Azure Service Bus publish operations.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Publishers;

/// <summary>Abstraction over Azure Service Bus integration event publish operations.</summary>
public interface IIntegrationEventPublisher
{
    /// <summary>Publishes an integration event to the configured topic.</summary>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class;
}
