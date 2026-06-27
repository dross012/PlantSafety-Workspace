// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.HumanFactors
//  Layer:          Infrastructure
//  File:           HumanFactorsConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the HumanFactors area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.HumanFactors;

/// <summary>
/// Registers all MassTransit consumers for the <c>HumanFactors</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class HumanFactorsConsumerRegistration
{
    /// <summary>Adds all HumanFactors consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
