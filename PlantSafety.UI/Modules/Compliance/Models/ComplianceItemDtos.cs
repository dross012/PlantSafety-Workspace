// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs mirroring the three Compliance
//   dashboard read-model endpoints (Compliance/RegulatoryRequirements,
//   Compliance/Obligations, Compliance/Compliance/Tracking). Each endpoint
//   returns an ApiResponse&lt;T&gt;-wrapped envelope whose Data payload has
//   this shape (TenantId/SiteId/GeneratedAt/Items) — the outer ApiResponse
//   envelope is intentionally not modeled here, matching the established
//   sibling pattern (Modules/PlanningProjects/Models/PlanningProjectsListResponseDto)
//   of deserializing directly into the inner Data shape.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Compliance.Models;

/// <summary>A single applicable regulatory requirement (GET v1/compliance/compliance/regulatoryrequirements).</summary>
public sealed class RequirementItemDto
{
    /// <summary>Requirement identifier.</summary>
    public Guid RequirementId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short title describing the requirement.</summary>
    public string RequirementTitle { get; init; } = string.Empty;

    /// <summary>Jurisdiction the requirement is enforced under (e.g., Federal, State, Local).</summary>
    public string Jurisdiction { get; init; } = string.Empty;

    /// <summary>Current requirement status (e.g., Applicable/Superseded/UnderReview).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the requirement became effective.</summary>
    public DateTime EffectiveAtUtc { get; init; }

    /// <summary>Identity of the user who owns tracking of the requirement.</summary>
    public Guid OwnerId { get; init; }
}

/// <summary>Response payload for the Regulatory Requirements read-model list endpoint.</summary>
public sealed class RequirementsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching requirement items. Never null; empty when no results.</summary>
    public IReadOnlyList<RequirementItemDto> Items { get; init; } = [];
}

/// <summary>A single compliance obligation owed to a regulatory body (GET v1/compliance/compliance/obligations).</summary>
public sealed class ObligationItemDto
{
    /// <summary>Obligation identifier.</summary>
    public Guid ObligationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short title describing the obligation.</summary>
    public string ObligationTitle { get; init; } = string.Empty;

    /// <summary>Regulatory body the obligation is owed to.</summary>
    public string RegulatoryBody { get; init; } = string.Empty;

    /// <summary>Current obligation status (e.g., Open/Fulfilled/Overdue).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the obligation is due.</summary>
    public DateTime DueAtUtc { get; init; }

    /// <summary>Identity of the user who owns the obligation.</summary>
    public Guid OwnerId { get; init; }
}

/// <summary>Response payload for the Obligations read-model list endpoint.</summary>
public sealed class ObligationsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching obligation items. Never null; empty when no results.</summary>
    public IReadOnlyList<ObligationItemDto> Items { get; init; } = [];
}

/// <summary>A single compliance tracking item (GET v1/compliance/compliance/tracking).</summary>
public sealed class TrackingItemDto
{
    /// <summary>Tracking item identifier.</summary>
    public Guid TrackingItemId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Description of the tracked item.</summary>
    public string ItemDescription { get; init; } = string.Empty;

    /// <summary>Category of the tracked item (e.g., Permit Renewal, Audit Action, Training).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Current tracking status (e.g., OnTrack/AtRisk/Overdue/Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the tracking item was last updated.</summary>
    public DateTime LastUpdatedAtUtc { get; init; }

    /// <summary>Identity of the user who owns the tracked item.</summary>
    public Guid OwnerId { get; init; }
}

/// <summary>Response payload for the Compliance Tracking read-model list endpoint.</summary>
public sealed class TrackingResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching tracking items. Never null; empty when no results.</summary>
    public IReadOnlyList<TrackingItemDto> Items { get; init; } = [];
}

// ============================================================
// Phase 5 §6.3 — Compliance Rules Engine (PlantSafety.Application.Compliance.RulesEngine.*)
// ============================================================

/// <summary>Read-model DTO for a single compliance violation detail (mirrors <c>ComplianceViolationDto</c>).</summary>
public sealed record ComplianceViolationDto
{
    /// <summary>Stable machine-readable violation code.</summary>
    public string ViolationCode { get; init; } = string.Empty;

    /// <summary>Human-readable description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Severity code (Advisory/Low/Medium/High/Critical).</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Optional remediation guidance.</summary>
    public string? RemediationGuidance { get; init; }
}

/// <summary>Read-model DTO for a single rule result recorded within a ComplianceEvaluation (mirrors <c>ComplianceRuleResultDto</c>).</summary>
public sealed record ComplianceRuleResultDto
{
    /// <summary>Stable machine-readable rule code.</summary>
    public string RuleCode { get; init; } = string.Empty;

    /// <summary>Human-readable rule name.</summary>
    public string RuleName { get; init; } = string.Empty;

    /// <summary>Governing category (Safety/Regulatory/InternalPolicy).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Whether the rule passed.</summary>
    public bool Passed { get; init; }

    /// <summary>Violation details raised by this rule; empty when <see cref="Passed"/> is true.</summary>
    public IReadOnlyList<ComplianceViolationDto> Violations { get; init; } = [];

    /// <summary>UTC instant the rule was evaluated.</summary>
    public DateTime EvaluatedAtUtc { get; init; }
}

/// <summary>Read-model DTO for a full ComplianceEvaluation run (mirrors <c>ComplianceEvaluationDto</c>).</summary>
public sealed record ComplianceEvaluationDto
{
    /// <summary>Evaluation identity.</summary>
    public Guid ComplianceEvaluationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type discriminator of the evaluated entity.</summary>
    public string TargetEntityType { get; init; } = string.Empty;

    /// <summary>Identity of the evaluated entity.</summary>
    public Guid TargetEntityId { get; init; }

    /// <summary>Overall outcome (InProgress/Compliant/NonCompliant).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Rule results recorded during this run.</summary>
    public IReadOnlyList<ComplianceRuleResultDto> RuleResults { get; init; } = [];

    /// <summary>UTC instant the run started.</summary>
    public DateTime StartedAtUtc { get; init; }

    /// <summary>UTC instant the run completed, if any.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>Identity of the user or system actor who triggered the run.</summary>
    public Guid TriggeredBy { get; init; }
}

/// <summary>Request body for <c>EvaluateComplianceCommand</c>'s caller-supplied fields.</summary>
public sealed record EvaluateComplianceRequest
{
    /// <summary>Optional site scope.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type discriminator of the entity being evaluated (e.g. "Risk", "Permit", "AuditFinding").</summary>
    public string TargetEntityType { get; init; } = string.Empty;

    /// <summary>Identity of the entity being evaluated.</summary>
    public Guid TargetEntityId { get; init; }

    /// <summary>Optional subset of rule categories to run ("Safety"/"Regulatory"/"InternalPolicy"). Null runs all three.</summary>
    public IReadOnlyCollection<string>? Categories { get; init; }

    /// <summary>Caller-gathered, already-resolved data points the rule pipeline needs.</summary>
    public IReadOnlyDictionary<string, string?> Facts { get; init; } = new Dictionary<string, string?>();
}
