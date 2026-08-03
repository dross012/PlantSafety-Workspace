// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.Training
//  Layer:          Infrastructure
//  File:           TrainingConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the Training area.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Training.CompetencyManagement.Messaging;

namespace PlantSafety.Infrastructure.Messaging.Consumers.Training;

/// <summary>
/// Registers all MassTransit consumers for the <c>Training</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class TrainingConsumerRegistration
{
    /// <summary>Adds all Training consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<CompetencyManagementCreatedConsumer>();
    }
}
