// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the AI Governance module. No
//   PlantSafety.Api read-model endpoint exists yet for this module today —
//   PlantSafety.Domain/AiGovernance is a thin Id/CreatedAtUtc/CreatedBy
//   scaffold aggregate with no queries/handlers/controllers wired. This shape
//   (governed model registry entries: governance status/risk tier/drift
//   status) is the best-inferable read-model DTO per the Intelligence.ML
//   Manual's model/feature/training/inference/drift governance rules
//   (ML1-4/D1-3) so the UI is ready to bind the instant a real endpoint
//   ships. Until then, AiGovernanceApiClient calls gracefully fault to the
//   page's Error state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.AiGovernance.Models;

/// <summary>Represents a single governed AI/ML model registry entry.</summary>
public sealed class GovernedModelDto
{
    /// <summary>Gets the unique identifier of the governed model.</summary>
    public Guid ModelId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the model display name.</summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>Gets the model version identifier.</summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>Gets the governance approval status (e.g., PendingReview/Approved/Rejected/Deprecated).</summary>
    public string GovernanceStatus { get; init; } = string.Empty;

    /// <summary>Gets the assessed risk tier (e.g., Low/Medium/High/Critical).</summary>
    public string RiskTier { get; init; } = string.Empty;

    /// <summary>Gets the current model drift status (e.g., Stable/Drifting/Critical).</summary>
    public string DriftStatus { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the most recent governance review.</summary>
    public DateTime LastReviewedAtUtc { get; init; }

    /// <summary>Gets the display name of the reviewer who last assessed this model.</summary>
    public string? ReviewerName { get; init; }
}

/// <summary>Envelope for the (not-yet-wired) GET v1/ai-governance/models endpoint.</summary>
public sealed class AiGovernanceResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the governed model items.</summary>
    public IReadOnlyList<GovernedModelDto> Items { get; init; } = [];
}
