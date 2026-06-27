// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.Compliance
//  Layer:          Infrastructure
//  File:           ComplianceConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the Compliance area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.Compliance;

/// <summary>
/// Registers all MassTransit consumers for the <c>Compliance</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class ComplianceConsumerRegistration
{
    /// <summary>Adds all Compliance consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
