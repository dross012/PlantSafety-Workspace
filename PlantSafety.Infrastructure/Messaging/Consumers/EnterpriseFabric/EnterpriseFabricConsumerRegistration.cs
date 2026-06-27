// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.EnterpriseFabric
//  Layer:          Infrastructure
//  File:           EnterpriseFabricConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the EnterpriseFabric area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.EnterpriseFabric;

/// <summary>
/// Registers all MassTransit consumers for the <c>EnterpriseFabric</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class EnterpriseFabricConsumerRegistration
{
    /// <summary>Adds all EnterpriseFabric consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
