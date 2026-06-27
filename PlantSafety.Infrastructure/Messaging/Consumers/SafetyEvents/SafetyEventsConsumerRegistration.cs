// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.SafetyEvents
//  Layer:          Infrastructure
//  File:           SafetyEventsConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the SafetyEvents area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.SafetyEvents;

/// <summary>
/// Registers all MassTransit consumers for the <c>SafetyEvents</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class SafetyEventsConsumerRegistration
{
    /// <summary>Adds all SafetyEvents consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
