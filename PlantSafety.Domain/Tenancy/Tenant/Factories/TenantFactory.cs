// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Default ITenantFactory implementation delegating to Tenant.Provision.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Tenancy.Tenant.Interfaces;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;

namespace PlantSafety.Domain.Tenancy.Tenant.Factories;

/// <summary>Default <see cref="ITenantFactory"/> implementation.</summary>
public sealed class TenantFactory : ITenantFactory
{
    /// <inheritdoc/>
    public Aggregates.Tenant Provision(
        string name, string region, SubscriptionPlanCode planCode,
        UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
        => Aggregates.Tenant.Provision(name, region, planCode, nowUtc, actor, correlationId, causationId);
}
