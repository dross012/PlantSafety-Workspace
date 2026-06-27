// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.Integrations
//  Layer:          Infrastructure
//  File:           IntegrationsConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the Integrations area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.Integrations;

/// <summary>
/// Registers all MassTransit consumers for the <c>Integrations</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class IntegrationsConsumerRegistration
{
    /// <summary>Adds all Integrations consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
