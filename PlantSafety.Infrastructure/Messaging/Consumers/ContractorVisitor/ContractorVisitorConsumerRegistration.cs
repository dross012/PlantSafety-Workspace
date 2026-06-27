// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.ContractorVisitor
//  Layer:          Infrastructure
//  File:           ContractorVisitorConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the ContractorVisitor area.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Messaging.Consumers.ContractorVisitor;

/// <summary>
/// Registers all MassTransit consumers for the <c>ContractorVisitor</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class ContractorVisitorConsumerRegistration
{
    /// <summary>Adds all ContractorVisitor consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // TODO: cfg.AddConsumer<{ModuleName}CreatedConsumer>();
    }
}
