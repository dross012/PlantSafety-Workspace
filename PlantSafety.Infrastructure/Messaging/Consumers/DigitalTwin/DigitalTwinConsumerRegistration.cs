// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.DigitalTwin
//  Layer:          Infrastructure
//  File:           DigitalTwinConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the DigitalTwin area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.DigitalTwin;

/// <summary>
/// Registers all MassTransit consumers for the <c>DigitalTwin</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class DigitalTwinConsumerRegistration
{
    /// <summary>Adds all DigitalTwin consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
