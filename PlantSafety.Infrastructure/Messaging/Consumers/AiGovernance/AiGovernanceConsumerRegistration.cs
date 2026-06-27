// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.AiGovernance
//  Layer:          Infrastructure
//  File:           AiGovernanceConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the AiGovernance area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.AiGovernance;

/// <summary>
/// Registers all MassTransit consumers for the <c>AiGovernance</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class AiGovernanceConsumerRegistration
{
    /// <summary>Adds all AiGovernance consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
