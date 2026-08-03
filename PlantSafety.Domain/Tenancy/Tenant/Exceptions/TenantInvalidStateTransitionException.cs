// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantInvalidStateTransitionException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.3 Tenant Lifecycle Management)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a lifecycle operation is attempted against a Tenant that is not in
//                  a valid state for it (e.g. reactivating a tenant that is not Suspended).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Domain.Tenancy.Tenant.Exceptions;

/// <summary>Thrown when a Tenant lifecycle operation is attempted from an invalid state.</summary>
public sealed class TenantInvalidStateTransitionException : TenantDomainException
{
    /// <summary>Creates an invalid-state-transition exception for the given Tenant identity.</summary>
    public TenantInvalidStateTransitionException(
        TenantId id, string currentStatus, string attemptedOperation,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot perform '{attemptedOperation}' on Tenant {id} while status is '{currentStatus}'.",
            "Tenant.InvalidStateTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
