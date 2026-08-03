// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Application.Enterprise.MultiRegion.DataResidency
//  Layer:          Application
//  File:           IDataResidencyEnforcer.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §5.3 (Data Residency) real, testable domain-rule enforcement point:
//                  given this platform's actual single-region deployment reality (no real
//                  multi-region infrastructure exists in this sandbox -- see
//                  docs\Phase7\multi-region-architecture.md), the one honest enforcement this
//                  codebase can perform is validating that a tenant's configured
//                  DataResidencyPolicy is actually satisfied by the platform's CURRENT deployment
//                  region. A real multi-region deployment (region-scoped SQL instances/AGs keyed
//                  by tenant affinity, per that document's §1) would instead ROUTE the tenant's
//                  data to a compliant region; until that infrastructure exists, this enforcer's
//                  job is to make any residency mismatch loud and visible (reject/flag) rather
//                  than silently accepting it. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Application.Enterprise.MultiRegion.DataResidency;

/// <summary>
/// Validates that a tenant's configured data-residency requirement is satisfied by the
/// platform's current deployment region.
/// </summary>
public interface IDataResidencyEnforcer
{
    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="tenantId"/>'s configured
    /// <see cref="Domain.MultiRegion.DataResidency.DataResidencyPolicy"/> is satisfied by the
    /// platform's current deployment region.
    /// </summary>
    bool IsSatisfied(Guid tenantId);

    /// <summary>
    /// Validates <paramref name="tenantId"/>'s data-residency requirement against the platform's
    /// current deployment region, throwing
    /// <see cref="Domain.MultiRegion.DataResidency.DataResidencyViolationException"/> if it is not
    /// satisfied.
    /// </summary>
    /// <exception cref="Domain.MultiRegion.DataResidency.DataResidencyViolationException">
    /// Thrown when the tenant's configured policy does not permit the current deployment region.
    /// </exception>
    void EnsureSatisfied(Guid tenantId, CorrelationId correlationId, CausationId causationId);
}
