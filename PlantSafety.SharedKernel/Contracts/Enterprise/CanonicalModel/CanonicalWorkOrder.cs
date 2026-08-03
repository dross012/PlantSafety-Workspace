// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.CanonicalModel
//  Layer:          SharedKernel
//  File:           CanonicalWorkOrder.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Canonical, enterprise-stable projection of a maintenance work order. Grounded
//                  in PlantSafety.Domain.Operational.WorkOrders.Aggregates.WorkOrder -- not a new
//                  parallel domain state. See README.md in this folder for the versioning
//                  convention and backward-compatibility rules. Additive-only evolution:
//                  CanonicalWorkOrderV2 only adds to CanonicalWorkOrderV1; CanonicalWorkOrderV3
//                  only adds to CanonicalWorkOrderV2.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

/// <summary>
/// V1 (stable) canonical projection of a work order: identity, key attributes, status, and due/
/// audit timestamps. The minimal shape external integration adapters should target first.
/// </summary>
public sealed record CanonicalWorkOrderV1
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 1;

    /// <summary>Stable enterprise identifier of the work order (mirrors the source WorkOrderId).</summary>
    public required Guid WorkOrderId { get; init; }

    /// <summary>Tenant that owns this work order.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this work order is scoped to, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable work order code (format WO-###).</summary>
    public required string WorkOrderCode { get; init; }

    /// <summary>Identifier of the asset/machine this work order targets (mirrors <see cref="CanonicalAssetV1.AssetId"/>).</summary>
    public required Guid AssetId { get; init; }

    /// <summary>Human-readable asset/machine code, captured for display.</summary>
    public required string AssetCode { get; init; }

    /// <summary>Priority classification.</summary>
    public required string Priority { get; init; }

    /// <summary>Description of the work to be performed.</summary>
    public required string Description { get; init; }

    /// <summary>Normalized current lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant the work order is due.</summary>
    public required UtcTimestamp DueAtUtc { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>UTC instant of the most recent update to this work order.</summary>
    public required UtcTimestamp UpdatedAtUtc { get; init; }
}

/// <summary>
/// V2 (extended) canonical projection of a work order. Additive-only over
/// <see cref="CanonicalWorkOrderV1"/>: adds assignment and lifecycle-completion detail already
/// tracked by the real WorkOrder aggregate.
/// </summary>
public sealed record CanonicalWorkOrderV2
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 2;

    /// <inheritdoc cref="CanonicalWorkOrderV1.WorkOrderId"/>
    public required Guid WorkOrderId { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.WorkOrderCode"/>
    public required string WorkOrderCode { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.AssetId"/>
    public required Guid AssetId { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.AssetCode"/>
    public required string AssetCode { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.Priority"/>
    public required string Priority { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.Description"/>
    public required string Description { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.DueAtUtc"/>
    public required UtcTimestamp DueAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.UpdatedAtUtc"/>
    public required UtcTimestamp UpdatedAtUtc { get; init; }

    /// <summary>Identity of the assigned technician, or null when unassigned.</summary>
    public Guid? TechnicianId { get; init; }

    /// <summary>Display name of the assigned technician, or null when unassigned.</summary>
    public string? TechnicianName { get; init; }

    /// <summary>UTC instant the technician was (last) assigned, if any.</summary>
    public UtcTimestamp? AssignedAtUtc { get; init; }

    /// <summary>UTC instant the work order transitioned to InProgress (most recent), if any.</summary>
    public UtcTimestamp? StartedAtUtc { get; init; }

    /// <summary>UTC instant the work order was last placed on hold, if any.</summary>
    public UtcTimestamp? OnHoldAtUtc { get; init; }

    /// <summary>UTC instant the work order was completed, if any.</summary>
    public UtcTimestamp? CompletedAtUtc { get; init; }

    /// <summary>Free-text notes recorded at completion, if any.</summary>
    public string? CompletionNotes { get; init; }

    /// <summary>UTC instant the work order was cancelled, if any.</summary>
    public UtcTimestamp? CancelledAtUtc { get; init; }

    /// <summary>Reason recorded when the work order was cancelled, if any.</summary>
    public string? CancelledReason { get; init; }
}

/// <summary>
/// V3 (predictive) canonical projection of a work order. Additive-only over
/// <see cref="CanonicalWorkOrderV2"/>: adds predicted-completion output wired to the real Phase 5
/// predictive-maintenance insights pipeline
/// (<see cref="PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors.FailureLikelihoodPredictor"/>
/// via Operational.PredictiveMaintenance, exposed today as
/// PlantSafety.Application.Scheduling.Rollup.Models.PredictiveInsightsDto). Populated only when the
/// owning module's mapper has a genuine predictive-insights result for the work order; otherwise
/// left null -- never fabricated.
/// </summary>
public sealed record CanonicalWorkOrderV3
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 3;

    /// <inheritdoc cref="CanonicalWorkOrderV1.WorkOrderId"/>
    public required Guid WorkOrderId { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.WorkOrderCode"/>
    public required string WorkOrderCode { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.AssetId"/>
    public required Guid AssetId { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.AssetCode"/>
    public required string AssetCode { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.Priority"/>
    public required string Priority { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.Description"/>
    public required string Description { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.DueAtUtc"/>
    public required UtcTimestamp DueAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV1.UpdatedAtUtc"/>
    public required UtcTimestamp UpdatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV2.TechnicianId"/>
    public Guid? TechnicianId { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV2.TechnicianName"/>
    public string? TechnicianName { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV2.AssignedAtUtc"/>
    public UtcTimestamp? AssignedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV2.StartedAtUtc"/>
    public UtcTimestamp? StartedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV2.OnHoldAtUtc"/>
    public UtcTimestamp? OnHoldAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV2.CompletedAtUtc"/>
    public UtcTimestamp? CompletedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV2.CompletionNotes"/>
    public string? CompletionNotes { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV2.CancelledAtUtc"/>
    public UtcTimestamp? CancelledAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalWorkOrderV2.CancelledReason"/>
    public string? CancelledReason { get; init; }

    /// <summary>Model-recommended completion instant (UTC), when a predictive-insights result exists.</summary>
    public UtcTimestamp? PredictedCompletionAtUtc { get; init; }

    /// <summary>Normalized predictive risk score (0.0-1.0) backing <see cref="PredictedCompletionAtUtc"/>.</summary>
    public double? PredictedRiskScore { get; init; }

    /// <summary>Traceable model name/version identifier for the prediction.</summary>
    public string? PredictionModelVersion { get; init; }

    /// <summary>UTC instant the prediction was generated.</summary>
    public UtcTimestamp? PredictionGeneratedAtUtc { get; init; }
}
