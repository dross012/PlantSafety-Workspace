// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.EnvironmentalHealth
//  Layer:          Infrastructure
//  File:           EnvironmentalHealthConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the EnvironmentalHealth area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.EnvironmentalHealth;

/// <summary>
/// Registers all MassTransit consumers for the <c>EnvironmentalHealth</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class EnvironmentalHealthConsumerRegistration
{
    /// <summary>Adds all EnvironmentalHealth consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
