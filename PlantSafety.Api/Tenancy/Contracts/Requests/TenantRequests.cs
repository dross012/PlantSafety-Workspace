// ============================================================================
// File: TenantRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
// Created: 2026-08-03
// Description: External-facing request DTOs for the Tenancy.Tenant module's V1 controller.
// ============================================================================

namespace PlantSafety.Api.Tenancy.Contracts.Requests;

/// <summary>Request body for provisioning a new tenant.</summary>
public sealed record ProvisionTenantRequest
{
    /// <summary>The new tenant's display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Data-residency/deployment region the tenant should be pinned to.</summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>Subscription plan reference code (e.g. "Trial", "Standard", "Professional", "Enterprise").</summary>
    public string PlanCode { get; init; } = "Trial";

    /// <summary>Email address for the tenant's initial administrative user.</summary>
    public string AdminEmail { get; init; } = string.Empty;

    /// <summary>First name for the tenant's initial administrative user.</summary>
    public string AdminFirstName { get; init; } = string.Empty;

    /// <summary>Last name for the tenant's initial administrative user.</summary>
    public string AdminLastName { get; init; } = string.Empty;
}

/// <summary>Request body for suspending a tenant.</summary>
public sealed record SuspendTenantRequest
{
    /// <summary>Human-readable reason for the suspension.</summary>
    public string Reason { get; init; } = string.Empty;
}

/// <summary>Request body for soft-deleting a tenant.</summary>
public sealed record DeleteTenantRequest
{
    /// <summary>Human-readable reason for the deletion.</summary>
    public string Reason { get; init; } = string.Empty;
}
