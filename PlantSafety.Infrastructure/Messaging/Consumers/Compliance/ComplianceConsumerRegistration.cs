// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.Compliance
//  Layer:          Infrastructure
//  File:           ComplianceConsumerRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration helper for the Compliance area.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Compliance.Auditing.Messaging;
using PlantSafety.Infrastructure.Compliance.Certification.Messaging;
using PlantSafety.Infrastructure.Compliance.Compliance.Messaging;
using PlantSafety.Infrastructure.Compliance.ManagementOfChange.Messaging;
using PlantSafety.Infrastructure.Compliance.Permits.Messaging;
using PlantSafety.Infrastructure.Compliance.Policies.Messaging;
using PlantSafety.Infrastructure.Compliance.Procedures.Messaging;
using PlantSafety.Infrastructure.Compliance.RegulatoryCalendar.Messaging;

namespace PlantSafety.Infrastructure.Messaging.Consumers.Compliance;

/// <summary>
/// Registers all MassTransit consumers for the <c>Compliance</c> area.
/// Called from <c>InfrastructureServiceRegistration</c> when configuring MassTransit.
/// </summary>
public static class ComplianceConsumerRegistration
{
    /// <summary>Adds all Compliance consumers to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<AuditingCreatedConsumer>();
        cfg.AddConsumer<CertificationCreatedConsumer>();
        cfg.AddConsumer<ComplianceCreatedConsumer>();
        cfg.AddConsumer<ManagementOfChangeCreatedConsumer>();
        cfg.AddConsumer<PermitsCreatedConsumer>();
        cfg.AddConsumer<PolicyCreatedConsumer>();
        cfg.AddConsumer<ProceduresCreatedConsumer>();
        cfg.AddConsumer<RegulatoryCalendarCreatedConsumer>();
    }
}
