// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MultiRegion.DataResidency
//  Layer:          Domain
//  File:           DataResidencyViolationException.cs
//  Created:        2026-08-03
//  Author:         Security & Compliance Agent (Phase 7 §5.3)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a tenant's configured DataResidencyPolicy does not permit the
//                  platform's current deployment region. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Exceptions;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.MultiRegion.DataResidency;

/// <summary>Thrown when a tenant's <see cref="DataResidencyPolicy"/> does not permit the platform's current deployment region.</summary>
public sealed class DataResidencyViolationException : DomainException
{
    /// <summary>The tenant whose residency requirement was violated.</summary>
    public Guid TenantId { get; }

    /// <summary>The tenant's configured residency requirement.</summary>
    public DataResidencyPolicy RequiredPolicy { get; }

    /// <summary>The platform's current deployment region, which does not satisfy <see cref="RequiredPolicy"/>.</summary>
    public Region CurrentDeploymentRegion { get; }

    /// <summary>Creates an exception describing the violation.</summary>
    public DataResidencyViolationException(
        Guid tenantId,
        DataResidencyPolicy requiredPolicy,
        Region currentDeploymentRegion,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            $"Tenant {tenantId}'s data-residency policy '{requiredPolicy.Code}' is not satisfied by the " +
            $"platform's current deployment region '{currentDeploymentRegion.Code}' (zone " +
            $"'{currentDeploymentRegion.DataResidencyZone}').",
            "MultiRegion.DataResidency.Violation",
            "Compliance",
            "High",
            correlationId,
            causationId)
    {
        TenantId = tenantId;
        RequiredPolicy = requiredPolicy;
        CurrentDeploymentRegion = currentDeploymentRegion;
    }
}
