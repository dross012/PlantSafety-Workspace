// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.Sustainability
//  Layer:          Infrastructure
//  File:           SustainabilityConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the Sustainability area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.Sustainability;

/// <summary>
/// Registers all MassTransit consumers for the <c>Sustainability</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class SustainabilityConsumerRegistration
{
    /// <summary>Adds all Sustainability consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
