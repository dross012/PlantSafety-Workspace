// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.Governance
//  Layer:          Infrastructure
//  File:           GovernanceConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the Governance area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.Governance;

/// <summary>
/// Registers all MassTransit consumers for the <c>Governance</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class GovernanceConsumerRegistration
{
    /// <summary>Adds all Governance consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
