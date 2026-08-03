// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh
//  Layer:          Infrastructure
//  File:           EnterpriseEventMeshConsumerRegistration.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3 Enterprise Event Mesh, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    MassTransit consumer registration for the Enterprise Event Mesh's outbound wiring
//                  points, mirroring the CrossModuleOrchestrationConsumerRegistration.Register(cfg)
//                  convention. Called from InfrastructureServiceRegistration alongside every other
//                  *ConsumerRegistration. Additive-only evolution.
//
//                  2026-08-03 UPDATE (Phase 6 §1 Enterprise System Integrations wave): added the
//                  ERP inventory/parts-consumption and financial-posting outbound wiring points, and
//                  the MES production-order-completion outbound wiring point. See
//                  PlantSafety.Infrastructure.Messaging.EnterpriseMesh.Erp/.Mes for each consumer's
//                  own header. Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.Messaging.EnterpriseMesh.Erp;
using PlantSafety.Infrastructure.Messaging.EnterpriseMesh.Mes;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh;

/// <summary>Registers every Enterprise Event Mesh outbound consumer with the MassTransit configurator.</summary>
public static class EnterpriseEventMeshConsumerRegistration
{
    /// <summary>Adds every Enterprise Event Mesh outbound consumer to the MassTransit configurator.</summary>
    public static void Register(IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<WorkOrderCompletedEnterpriseMeshConsumer>();

        // Phase 6 §1.1 ERP Integration (2026-08-03).
        cfg.AddConsumer<PartStockConsumptionErpMeshConsumer>();
        cfg.AddConsumer<FinancialRecordApprovedErpMeshConsumer>();

        // Phase 6 §1.2 MES Integration (2026-08-03).
        cfg.AddConsumer<ProductionRunCompletedMesMeshConsumer>();
    }
}
