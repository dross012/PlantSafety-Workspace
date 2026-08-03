// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.CanonicalModel
//  Layer:          SharedKernel
//  File:           CanonicalProductionOrder.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Canonical, enterprise-stable projection of a production run. Grounded in
//                  PlantSafety.Domain.Operational.ProductionTracking.Aggregates.ProductionRun --
//                  not a new parallel domain state. See README.md in this folder for the
//                  versioning convention and backward-compatibility rules. Additive-only
//                  evolution: CanonicalProductionOrderV2 only adds to CanonicalProductionOrderV1;
//                  CanonicalProductionOrderV3 only adds to CanonicalProductionOrderV2.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

/// <summary>
/// V1 (stable) canonical projection of a production order/run: identity, key attributes, status,
/// and start/audit timestamps. The minimal shape external integration adapters should target
/// first.
/// </summary>
public sealed record CanonicalProductionOrderV1
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 1;

    /// <summary>Stable enterprise identifier of the production order (mirrors the source ProductionRunId).</summary>
    public required Guid ProductionOrderId { get; init; }

    /// <summary>Tenant that owns this production order.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this production order is scoped to, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable run code (format PR-###).</summary>
    public required string OrderCode { get; init; }

    /// <summary>Identifier of the ProductionLine master-data record this order executes on.</summary>
    public required Guid ProductionLineId { get; init; }

    /// <summary>Display name of the production line, captured for read-model convenience.</summary>
    public required string ProductionLineName { get; init; }

    /// <summary>Identifier of the asset/machine driving this production order (mirrors <see cref="CanonicalAssetV1.AssetId"/>).</summary>
    public required Guid AssetId { get; init; }

    /// <summary>Normalized current lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant the production order started.</summary>
    public required UtcTimestamp StartedAtUtc { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>UTC instant of the most recent update to this production order.</summary>
    public required UtcTimestamp UpdatedAtUtc { get; init; }
}

/// <summary>
/// V2 (extended) canonical projection of a production order/run. Additive-only over
/// <see cref="CanonicalProductionOrderV1"/>: adds operator and running-totals detail already
/// tracked by the real ProductionRun aggregate.
/// </summary>
public sealed record CanonicalProductionOrderV2
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 2;

    /// <inheritdoc cref="CanonicalProductionOrderV1.ProductionOrderId"/>
    public required Guid ProductionOrderId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.OrderCode"/>
    public required string OrderCode { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.ProductionLineId"/>
    public required Guid ProductionLineId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.ProductionLineName"/>
    public required string ProductionLineName { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.AssetId"/>
    public required Guid AssetId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.StartedAtUtc"/>
    public required UtcTimestamp StartedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.UpdatedAtUtc"/>
    public required UtcTimestamp UpdatedAtUtc { get; init; }

    /// <summary>Operator display name, when recorded.</summary>
    public string? OperatorName { get; init; }

    /// <summary>Rated/nameplate ideal cycle time (seconds) for this order, when recorded.</summary>
    public double? IdealCycleTimeSeconds { get; init; }

    /// <summary>Cumulative count of units produced that passed quality inspection.</summary>
    public int? GoodUnitCount { get; init; }

    /// <summary>Cumulative count of units scrapped/rejected.</summary>
    public int? ScrapUnitCount { get; init; }

    /// <summary>Cumulative downtime minutes recorded against this order.</summary>
    public double? DowntimeMinutesTotal { get; init; }

    /// <summary>UTC instant the order ended (Completed/Failed), if any.</summary>
    public UtcTimestamp? EndedAtUtc { get; init; }
}

/// <summary>
/// V3 (predictive) canonical projection of a production order/run. Additive-only over
/// <see cref="CanonicalProductionOrderV2"/>. No dedicated ML predictor exists for production-order
/// completion in the Phase 5 predictor infrastructure; this version instead reuses the real,
/// general-purpose
/// <see cref="PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors.TaskDurationPredictor"/>
/// heuristic (historical average duration adjusted by complexity/crew-experience factors) against
/// the order's own historical cycle-time data when the owning module's mapper has genuine inputs
/// for it. Left null otherwise -- never fabricated.
/// </summary>
public sealed record CanonicalProductionOrderV3
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 3;

    /// <inheritdoc cref="CanonicalProductionOrderV1.ProductionOrderId"/>
    public required Guid ProductionOrderId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.OrderCode"/>
    public required string OrderCode { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.ProductionLineId"/>
    public required Guid ProductionLineId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.ProductionLineName"/>
    public required string ProductionLineName { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.AssetId"/>
    public required Guid AssetId { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.StartedAtUtc"/>
    public required UtcTimestamp StartedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV1.UpdatedAtUtc"/>
    public required UtcTimestamp UpdatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV2.OperatorName"/>
    public string? OperatorName { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV2.IdealCycleTimeSeconds"/>
    public double? IdealCycleTimeSeconds { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV2.GoodUnitCount"/>
    public int? GoodUnitCount { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV2.ScrapUnitCount"/>
    public int? ScrapUnitCount { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV2.DowntimeMinutesTotal"/>
    public double? DowntimeMinutesTotal { get; init; }

    /// <inheritdoc cref="CanonicalProductionOrderV2.EndedAtUtc"/>
    public UtcTimestamp? EndedAtUtc { get; init; }

    /// <summary>
    /// Predicted completion instant (UTC), from <see cref="PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors.TaskDurationPredictor"/>
    /// applied to this order's historical cycle-time data. Null when no prediction has been computed.
    /// </summary>
    public UtcTimestamp? PredictedCompletionAtUtc { get; init; }

    /// <summary>Model confidence (0.0-1.0) backing <see cref="PredictedCompletionAtUtc"/>.</summary>
    public double? PredictedCompletionConfidence { get; init; }

    /// <summary>Traceable model name/version identifier for the prediction.</summary>
    public string? PredictionModelVersion { get; init; }

    /// <summary>UTC instant the prediction was generated.</summary>
    public UtcTimestamp? PredictionGeneratedAtUtc { get; init; }
}
