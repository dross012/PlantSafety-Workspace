// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration
//  Layer:          Infrastructure
//  File:           CrossModuleOrchestrationConsumerRegistration.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3 Cross-Module Orchestration)
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration for all 8 Phase 5 §3 cross-module
//                  orchestration wiring points (§3.1 Maintenance <-> Predictive Maintenance,
//                  §3.2 Production Tracking <-> Scheduling, §3.3 RiskManagement <->
//                  ContinuousImprovement, §3.4 Intelligence <-> everything). Called from
//                  InfrastructureServiceRegistration when configuring MassTransit, mirroring the
//                  per-module *ConsumerRegistration.Register(cfg) convention established elsewhere
//                  under Messaging.Consumers. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation;
using PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.MaintenancePrediction;
using PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.ProductionScheduling;
using PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.RiskContinuousImprovement;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration;

/// <summary>
/// Registers all Phase 5 §3 cross-module orchestration consumers with the MassTransit
/// configurator.
/// </summary>
public static class CrossModuleOrchestrationConsumerRegistration
{
    /// <summary>Adds every cross-module orchestration consumer to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        // §3.1 Maintenance <-> Predictive Maintenance
        cfg.AddConsumer<PredictiveMaintenanceEscalationConsumer>();
        cfg.AddConsumer<MaintenanceCompletionFeedbackConsumer>();

        // §3.2 Production Tracking <-> Scheduling
        cfg.AddConsumer<ProductionAnomalyReprioritizationConsumer>();
        cfg.AddConsumer<SchedulingKpiFeedbackConsumer>();

        // §3.3 RiskManagement <-> ContinuousImprovement
        cfg.AddConsumer<RiskEscalationCapaConsumer>();
        cfg.AddConsumer<CapaCompletionRiskReductionConsumer>();

        // §3.4 Intelligence <-> everything
        cfg.AddConsumer<InsufficientMitigationInsightConsumer>();
        cfg.AddConsumer<PredictiveSafetyAlertDashboardFeedConsumer>();
    }
}
