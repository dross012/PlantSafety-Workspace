// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.WorkerWellBeing
//  Layer:          Infrastructure
//  File:           WorkerWellBeingConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the WorkerWellBeing area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.WorkerWellBeing;

/// <summary>
/// Registers all MassTransit consumers for the <c>WorkerWellBeing</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class WorkerWellBeingConsumerRegistration
{
    /// <summary>Adds all WorkerWellBeing consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
