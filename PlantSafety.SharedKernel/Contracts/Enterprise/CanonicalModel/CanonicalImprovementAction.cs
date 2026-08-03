// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.CanonicalModel
//  Layer:          SharedKernel
//  File:           CanonicalImprovementAction.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Canonical, enterprise-stable projection of a corrective/preventive action
//                  (CAPA). Grounded in
//                  PlantSafety.Domain.ContinuousImprovement.CAPA.Aggregates.CorrectiveAction --
//                  not a new parallel domain state. See README.md in this folder for the
//                  versioning convention and backward-compatibility rules. Additive-only
//                  evolution: CanonicalImprovementActionV2 only adds to
//                  CanonicalImprovementActionV1; CanonicalImprovementActionV3 only adds to
//                  CanonicalImprovementActionV2.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

/// <summary>
/// V1 (stable) canonical projection of an improvement action: identity, key attributes, status,
/// and due/creation timestamps. The minimal shape external integration adapters should target
/// first.
/// </summary>
public sealed record CanonicalImprovementActionV1
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 1;

    /// <summary>Stable enterprise identifier of the action (mirrors the source CorrectiveActionId).</summary>
    public required Guid ImprovementActionId { get; init; }

    /// <summary>Tenant that owns this action record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Identity of the source safety event that triggered this action (e.g. IncidentId, AuditFindingId, NearMissId).</summary>
    public required Guid SourceEventId { get; init; }

    /// <summary>Type discriminator of the source event (Incident, NearMiss, AuditFinding, Observation).</summary>
    public required string SourceEventType { get; init; }

    /// <summary>Title of the improvement action.</summary>
    public required string Title { get; init; }

    /// <summary>Description of the improvement action.</summary>
    public required string Description { get; init; }

    /// <summary>Action type: Corrective (fixes a found problem) or Preventive (prevents recurrence).</summary>
    public required string ActionType { get; init; }

    /// <summary>Priority (e.g. Low, Medium, High, Critical).</summary>
    public required string Priority { get; init; }

    /// <summary>Normalized current lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>Identity of the worker assigned to implement this action.</summary>
    public required Guid AssignedToWorkerId { get; init; }

    /// <summary>UTC due date for completion.</summary>
    public required UtcTimestamp DueAtUtc { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public required UtcTimestamp CreatedAtUtc { get; init; }
}

/// <summary>
/// V2 (extended) canonical projection of an improvement action. Additive-only over
/// <see cref="CanonicalImprovementActionV1"/>: adds completion detail already tracked by the real
/// CorrectiveAction aggregate.
/// </summary>
public sealed record CanonicalImprovementActionV2
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 2;

    /// <inheritdoc cref="CanonicalImprovementActionV1.ImprovementActionId"/>
    public required Guid ImprovementActionId { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.SourceEventId"/>
    public required Guid SourceEventId { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.SourceEventType"/>
    public required string SourceEventType { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.Title"/>
    public required string Title { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.Description"/>
    public required string Description { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.ActionType"/>
    public required string ActionType { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.Priority"/>
    public required string Priority { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.AssignedToWorkerId"/>
    public required Guid AssignedToWorkerId { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.DueAtUtc"/>
    public required UtcTimestamp DueAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>UTC instant this action was marked Completed (moved to AwaitingVerification), if any.</summary>
    public UtcTimestamp? CompletedAtUtc { get; init; }
}

/// <summary>
/// V3 (forward-looking) canonical projection of an improvement action. Additive-only over
/// <see cref="CanonicalImprovementActionV2"/>. No genuine ML/heuristic predictor exists for CAPA
/// recurrence/overdue-risk in the Phase 5 predictor infrastructure; this version instead surfaces
/// a deterministic overdue projection computed with the same guard condition the real
/// <c>CorrectiveAction.MarkOverdue</c> lifecycle method already enforces -- not an ML prediction,
/// and clearly documented as such.
/// </summary>
public sealed record CanonicalImprovementActionV3
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 3;

    /// <inheritdoc cref="CanonicalImprovementActionV1.ImprovementActionId"/>
    public required Guid ImprovementActionId { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.SourceEventId"/>
    public required Guid SourceEventId { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.SourceEventType"/>
    public required string SourceEventType { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.Title"/>
    public required string Title { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.Description"/>
    public required string Description { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.ActionType"/>
    public required string ActionType { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.Priority"/>
    public required string Priority { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.AssignedToWorkerId"/>
    public required Guid AssignedToWorkerId { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.DueAtUtc"/>
    public required UtcTimestamp DueAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalImprovementActionV2.CompletedAtUtc"/>
    public UtcTimestamp? CompletedAtUtc { get; init; }

    /// <summary>
    /// True when this action's due date has passed while it is still Open/InProgress, computed
    /// deterministically (mirrors the real <c>CorrectiveAction.MarkOverdue</c> guard condition;
    /// not an ML prediction).
    /// </summary>
    public bool? IsPredictedOverdue { get; init; }
}
