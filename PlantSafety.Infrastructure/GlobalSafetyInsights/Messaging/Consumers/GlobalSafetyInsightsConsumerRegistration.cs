// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Infrastructure
//  File:           GlobalSafetyInsightsConsumerRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration for the GlobalSafetyInsights event-mesh
//                  extension, mirroring the per-module *ConsumerRegistration.Register(cfg)
//                  convention (see CrossModuleOrchestrationConsumerRegistration). Called from
//                  InfrastructureServiceRegistration when configuring MassTransit. Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.GlobalSafetyInsights.Messaging.Consumers;

/// <summary>Registers every GlobalSafetyInsights event-mesh consumer with the MassTransit configurator.</summary>
public static class GlobalSafetyInsightsConsumerRegistration
{
    /// <summary>Adds the GlobalSafetyInsights consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<GlobalSafetyTelemetryConsumer>();
        cfg.AddConsumer<CrossSiteActionConsumer>();
    }
}
