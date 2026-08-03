// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           ITenantFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Domain-layer factory abstraction for constructing new Tenant aggregates,
//                  mirroring the platform's established IAuditReportFactory/
//                  IOptimizationRunFactory precedent. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;

namespace PlantSafety.Domain.Tenancy.Tenant.Interfaces;

/// <summary>Domain-layer factory abstraction for constructing new Tenant aggregates.</summary>
public interface ITenantFactory
{
    /// <summary>Provisions a new Tenant aggregate in the Provisioning status.</summary>
    Aggregates.Tenant Provision(
        string name, string region, SubscriptionPlanCode planCode,
        UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId);
}
