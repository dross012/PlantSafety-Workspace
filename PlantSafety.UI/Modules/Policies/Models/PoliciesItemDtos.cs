// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs mirroring the Policies & Procedures
//   read-model endpoints (Policy/SafetyPolicies, Procedures/SOPs).
//   Deserialized directly into the inner Data shape of the ApiResponse&lt;T&gt;
//   envelope, matching the established sibling convention (see
//   Modules/PlanningProjects/Models/PlanningProjectsListResponseDto.cs).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Policies.Models;

/// <summary>A single Safety Policy document (GET v1/compliance/policy/safetypolicies).</summary>
public sealed class PolicyItemDto
{
    /// <summary>Policy identifier.</summary>
    public Guid PolicyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Policy name.</summary>
    public string PolicyName { get; init; } = string.Empty;

    /// <summary>Policy category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Current approval status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the policy became effective.</summary>
    public DateTime EffectiveAtUtc { get; init; }

    /// <summary>Identity of the user who owns the policy.</summary>
    public Guid OwnerId { get; init; }
}

/// <summary>Response payload for the Safety Policies read-model list endpoint.</summary>
public sealed class PoliciesResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching policy items. Never null; empty when no results.</summary>
    public IReadOnlyList<PolicyItemDto> Items { get; init; } = [];
}

/// <summary>A single Standard Operating Procedure (GET v1/compliance/procedures/sops).</summary>
public sealed class ProcedureItemDto
{
    /// <summary>SOP identifier.</summary>
    public Guid SOPId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>SOP title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Process area the SOP governs.</summary>
    public string ProcessArea { get; init; } = string.Empty;

    /// <summary>Current approval status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the SOP became effective.</summary>
    public DateTime EffectiveAtUtc { get; init; }

    /// <summary>Identity of the user who owns the SOP.</summary>
    public Guid OwnerId { get; init; }
}

/// <summary>Response payload for the SOPs read-model list endpoint.</summary>
public sealed class ProceduresResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching SOP items. Never null; empty when no results.</summary>
    public IReadOnlyList<ProcedureItemDto> Items { get; init; } = [];
}
