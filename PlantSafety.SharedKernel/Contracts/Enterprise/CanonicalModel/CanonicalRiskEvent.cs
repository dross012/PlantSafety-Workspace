// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.CanonicalModel
//  Layer:          SharedKernel
//  File:           CanonicalRiskEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Canonical, enterprise-stable projection of an identified/tracked risk. Grounded
//                  in PlantSafety.Domain.RiskManagement.Aggregates.Risk -- not a new parallel
//                  domain state. See README.md in this folder for the versioning convention and
//                  backward-compatibility rules. Additive-only evolution: CanonicalRiskEventV2
//                  only adds to CanonicalRiskEventV1; CanonicalRiskEventV3 only adds to
//                  CanonicalRiskEventV2.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

/// <summary>
/// V1 (stable) canonical projection of a risk event: identity, key hazard attributes, status, and
/// creation timestamp. The minimal shape external integration adapters should target first.
/// </summary>
public sealed record CanonicalRiskEventV1
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 1;

    /// <summary>Stable enterprise identifier of the risk event (mirrors the source RiskId).</summary>
    public required Guid RiskEventId { get; init; }

    /// <summary>Tenant that owns this risk record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this risk is scoped to, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable hazard title/summary.</summary>
    public required string RiskTitle { get; init; }

    /// <summary>Qualitative risk level/category (Low/Medium/High/Critical).</summary>
    public required string Category { get; init; }

    /// <summary>Free-form location description (building/zone/equipment) where the hazard exists.</summary>
    public required string Location { get; init; }

    /// <summary>Hazard source description (mechanical, electrical, chemical, ergonomic, process, human factors).</summary>
    public required string HazardSource { get; init; }

    /// <summary>Normalized current lifecycle status within the 3-phase risk management process.</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant this risk record was created/identified.</summary>
    public required UtcTimestamp CreatedAtUtc { get; init; }
}

/// <summary>
/// V2 (extended) canonical projection of a risk event. Additive-only over
/// <see cref="CanonicalRiskEventV1"/>: adds scoring/mitigation/review detail already tracked by
/// the real Risk aggregate.
/// </summary>
public sealed record CanonicalRiskEventV2
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 2;

    /// <inheritdoc cref="CanonicalRiskEventV1.RiskEventId"/>
    public required Guid RiskEventId { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.RiskTitle"/>
    public required string RiskTitle { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.Category"/>
    public required string Category { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.Location"/>
    public required string Location { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.HazardSource"/>
    public required string HazardSource { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Assigned P1-P4 remediation priority, once prioritized.</summary>
    public string? Priority { get; init; }

    /// <summary>Severity component (1-5) of the most recently recorded risk score, once assessed.</summary>
    public int? CurrentScoreSeverity { get; init; }

    /// <summary>Likelihood component (1-5) of the most recently recorded risk score, once assessed.</summary>
    public int? CurrentScoreLikelihood { get; init; }

    /// <summary>Severity component (1-5) of the residual risk score, once validated.</summary>
    public int? ResidualScoreSeverity { get; init; }

    /// <summary>Likelihood component (1-5) of the residual risk score, once validated.</summary>
    public int? ResidualScoreLikelihood { get; init; }

    /// <summary>Count of control measures applied against this risk.</summary>
    public int? ControlMeasureCount { get; init; }

    /// <summary>Mandated review cadence code (e.g. Monthly/Quarterly/Annually).</summary>
    public string? ReviewCadenceCode { get; init; }

    /// <summary>UTC instant of the most recently completed periodic review, if any.</summary>
    public UtcTimestamp? LastReviewedAtUtc { get; init; }
}

/// <summary>
/// V3 (forward-looking) canonical projection of a risk event. Additive-only over
/// <see cref="CanonicalRiskEventV2"/>. No genuine ML/heuristic predictor exists for risk-event
/// recurrence/escalation in the Phase 5 predictor infrastructure; this version instead surfaces
/// the real, already-tracked <c>Risk.NextReviewDueUtc</c> field and a deterministic overdue flag
/// computed with the same comparison the real aggregate's review workflow relies on -- not an ML
/// prediction, and clearly documented as such.
/// </summary>
public sealed record CanonicalRiskEventV3
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 3;

    /// <inheritdoc cref="CanonicalRiskEventV1.RiskEventId"/>
    public required Guid RiskEventId { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.RiskTitle"/>
    public required string RiskTitle { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.Category"/>
    public required string Category { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.Location"/>
    public required string Location { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.HazardSource"/>
    public required string HazardSource { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV2.Priority"/>
    public string? Priority { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV2.CurrentScoreSeverity"/>
    public int? CurrentScoreSeverity { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV2.CurrentScoreLikelihood"/>
    public int? CurrentScoreLikelihood { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV2.ResidualScoreSeverity"/>
    public int? ResidualScoreSeverity { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV2.ResidualScoreLikelihood"/>
    public int? ResidualScoreLikelihood { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV2.ControlMeasureCount"/>
    public int? ControlMeasureCount { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV2.ReviewCadenceCode"/>
    public string? ReviewCadenceCode { get; init; }

    /// <inheritdoc cref="CanonicalRiskEventV2.LastReviewedAtUtc"/>
    public UtcTimestamp? LastReviewedAtUtc { get; init; }

    /// <summary>UTC instant the next periodic review is due, mirroring the real Risk aggregate's own field.</summary>
    public DateTime? NextReviewDueUtc { get; init; }

    /// <summary>
    /// True when <see cref="NextReviewDueUtc"/> has passed as of the mapping instant. A
    /// deterministic projection (not ML-derived) of the same condition the real review workflow
    /// evaluates.
    /// </summary>
    public bool? IsReviewOverdue { get; init; }
}
