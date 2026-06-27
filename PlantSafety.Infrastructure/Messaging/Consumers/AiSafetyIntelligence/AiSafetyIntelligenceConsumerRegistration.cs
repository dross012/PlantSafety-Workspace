// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.AiSafetyIntelligence
//  Layer:          Infrastructure
//  File:           AiSafetyIntelligenceConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the AiSafetyIntelligence area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.AiSafetyIntelligence;

/// <summary>
/// Registers all MassTransit consumers for the <c>AiSafetyIntelligence</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class AiSafetyIntelligenceConsumerRegistration
{
    /// <summary>Adds all AiSafetyIntelligence consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
