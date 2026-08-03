// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Domain
//  File:           TenantNotFoundException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Tenant aggregate lookup finds no match. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Domain.Tenancy.Tenant.Exceptions;

/// <summary>Thrown when a Tenant aggregate lookup finds no match.</summary>
public sealed class TenantNotFoundException : TenantDomainException
{
    /// <summary>Creates a not-found exception for the given Tenant identity.</summary>
    public TenantNotFoundException(TenantId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"Tenant with id '{id}' was not found.",
            "Tenant.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
