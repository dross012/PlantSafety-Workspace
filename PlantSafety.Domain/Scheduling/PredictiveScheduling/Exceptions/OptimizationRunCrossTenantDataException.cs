// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunCrossTenantDataException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §1.4 Tenant-Aware Scheduling)
//  SchemaVersion:  2026.1
//  Description:    Thrown when OptimizationRun.Request is supplied a SchedulableTask or
//                  ResourceAvailabilityWindow whose (populated) TenantId does not match the
//                  requesting run's own TenantId -- guards against cross-tenant data ever being
//                  mixed into a single optimization run. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Exceptions;

/// <summary>Thrown when an OptimizationRun request would mix SchedulableTask/ResourceAvailabilityWindow records across different tenants.</summary>
public sealed class OptimizationRunCrossTenantDataException : OptimizationRunDomainException
{
    /// <summary>Creates a cross-tenant-data exception for the given run's tenant and the offending record's tenant.</summary>
    public OptimizationRunCrossTenantDataException(
        Guid runTenantId, Guid offendingTenantId, CorrelationId correlationId, CausationId causationId)
        : base(
            $"OptimizationRun request for tenant '{runTenantId}' included a record tagged with a different tenant '{offendingTenantId}'.",
            "OptimizationRun.CrossTenantData",
            "TenantIsolation",
            "Critical",
            correlationId,
            causationId)
    {
    }
}
