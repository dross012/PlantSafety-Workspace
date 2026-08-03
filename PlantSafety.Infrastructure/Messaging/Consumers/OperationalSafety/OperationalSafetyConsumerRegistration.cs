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
using PlantSafety.Infrastructure.OperationalSafety.Alerts.Messaging;
using PlantSafety.Infrastructure.OperationalSafety.Checklists.Messaging;
using PlantSafety.Infrastructure.OperationalSafety.PPEManagement.Messaging;
using PlantSafety.Infrastructure.OperationalSafety.RiskManagement.Messaging;
using PlantSafety.Infrastructure.OperationalSafety.Schedules.Messaging;

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
        cfg.AddConsumer<AlertsCreatedConsumer>();
        cfg.AddConsumer<ChecklistsCreatedConsumer>();
        cfg.AddConsumer<PPEManagementCreatedConsumer>();
        cfg.AddConsumer<RiskManagementCreatedConsumer>();
        cfg.AddConsumer<SchedulesCreatedConsumer>();
    }
}
