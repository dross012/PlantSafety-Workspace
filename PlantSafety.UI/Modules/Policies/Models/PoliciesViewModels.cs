// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view models for the Policies & Procedures
//   module's two tabs (Policies/Procedures).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Policies.Models;

/// <summary>List-row and detail view model for a single Safety Policy document.</summary>
public sealed class PolicyDetailViewModel
{
    /// <summary>Gets the policy identifier.</summary>
    public required Guid PolicyId { get; init; }

    /// <summary>Gets the policy name.</summary>
    public required string PolicyName { get; init; }

    /// <summary>Gets the policy category.</summary>
    public required string Category { get; init; }

    /// <summary>Gets the raw approval status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the UTC timestamp when the policy became effective.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Gets the identifier of the user who owns the policy.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
}

/// <summary>List-row and detail view model for a single Standard Operating Procedure.</summary>
public sealed class ProcedureDetailViewModel
{
    /// <summary>Gets the SOP identifier.</summary>
    public required Guid SOPId { get; init; }

    /// <summary>Gets the SOP title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the process area the SOP governs.</summary>
    public required string ProcessArea { get; init; }

    /// <summary>Gets the raw approval status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the UTC timestamp when the SOP became effective.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Gets the identifier of the user who owns the SOP.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
}
