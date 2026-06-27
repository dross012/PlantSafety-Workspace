// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.OperationalSafety
//  Layer:          Infrastructure
//  File:           OperationalSafetyConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the OperationalSafety area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.OperationalSafety;

/// <summary>
/// Registers all MassTransit consumers for the <c>OperationalSafety</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class OperationalSafetyConsumerRegistration
{
    /// <summary>Adds all OperationalSafety consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
