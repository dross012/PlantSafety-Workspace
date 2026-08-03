// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.CanonicalModel
//  Layer:          SharedKernel
//  File:           CanonicalAsset.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Canonical, enterprise-stable projection of equipment/machine identity --
//                  the platform's "Asset" concept for enterprise integration (ERP/MES/CMMS
//                  adapters, the event mesh). Grounded in the real equipment-identity read model
//                  (PlantSafety.Application.CrossDomain.Machines.Models.MachinesItemDto) and the
//                  Operational.EquipmentSafety module -- not a new parallel domain state. See
//                  README.md in this folder for the versioning convention and backward-
//                  compatibility rules. Additive-only evolution: CanonicalAssetV2 only adds to
//                  CanonicalAssetV1; CanonicalAssetV3 only adds to CanonicalAssetV2.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

/// <summary>
/// V1 (stable) canonical projection of an equipment/machine asset: identity, key attributes,
/// status, and creation/update timestamps. The minimal shape external integration adapters
/// should target first.
/// </summary>
public sealed record CanonicalAssetV1
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 1;

    /// <summary>Stable enterprise identifier of the asset (mirrors the source MachineId).</summary>
    public required Guid AssetId { get; init; }

    /// <summary>Tenant that owns this asset record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site this asset is scoped to, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable asset/machine display name.</summary>
    public required string AssetName { get; init; }

    /// <summary>Asset type/class (e.g. Press/Conveyor/CNC).</summary>
    public required string AssetType { get; init; }

    /// <summary>Normalized current operating status (e.g. Operational/Maintenance/OutOfService).</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant this asset record was created/registered.</summary>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>UTC instant of the most recent update to this asset record.</summary>
    public required UtcTimestamp UpdatedAtUtc { get; init; }
}

/// <summary>
/// V2 (extended) canonical projection of an equipment/machine asset. Additive-only over
/// <see cref="CanonicalAssetV1"/>: adds serial number and last-service detail already tracked by
/// the real Machines read model.
/// </summary>
public sealed record CanonicalAssetV2
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 2;

    /// <inheritdoc cref="CanonicalAssetV1.AssetId"/>
    public required Guid AssetId { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.AssetName"/>
    public required string AssetName { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.AssetType"/>
    public required string AssetType { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.UpdatedAtUtc"/>
    public required UtcTimestamp UpdatedAtUtc { get; init; }

    /// <summary>Manufacturer serial number, when known.</summary>
    public string? SerialNumber { get; init; }

    /// <summary>UTC timestamp of the asset's last recorded service, when known.</summary>
    public UtcTimestamp? LastServicedAtUtc { get; init; }

    /// <summary>Identity of the technician who last serviced the asset, when known.</summary>
    public Guid? LastServicedBy { get; init; }
}

/// <summary>
/// V3 (predictive) canonical projection of an equipment/machine asset. Additive-only over
/// <see cref="CanonicalAssetV2"/>: adds failure-likelihood output wired to the real Phase 5
/// <see cref="PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors.FailureLikelihoodPredictor"/>
/// heuristic model (not fabricated -- populated only when the owning module's mapper has genuine
/// input features for the predictor; otherwise left null).
/// </summary>
public sealed record CanonicalAssetV3
{
    /// <summary>Schema version of this canonical shape.</summary>
    public const int SchemaVersion = 3;

    /// <inheritdoc cref="CanonicalAssetV1.AssetId"/>
    public required Guid AssetId { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.TenantId"/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.SiteId"/>
    public Guid? SiteId { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.AssetName"/>
    public required string AssetName { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.AssetType"/>
    public required string AssetType { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.Status"/>
    public required string Status { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.CreatedAtUtc"/>
    public required UtcTimestamp CreatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalAssetV1.UpdatedAtUtc"/>
    public required UtcTimestamp UpdatedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalAssetV2.SerialNumber"/>
    public string? SerialNumber { get; init; }

    /// <inheritdoc cref="CanonicalAssetV2.LastServicedAtUtc"/>
    public UtcTimestamp? LastServicedAtUtc { get; init; }

    /// <inheritdoc cref="CanonicalAssetV2.LastServicedBy"/>
    public Guid? LastServicedBy { get; init; }

    /// <summary>
    /// Estimated likelihood (0.0-1.0) of failure before the next scheduled PM window, from
    /// <see cref="PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors.FailureLikelihoodPredictor"/>.
    /// Null when no prediction has been computed for this asset.
    /// </summary>
    public double? PredictedFailureLikelihood { get; init; }

    /// <summary>True when <see cref="PredictedFailureLikelihood"/> crosses the platform alerting threshold.</summary>
    public bool? ShouldRaiseFailureAlert { get; init; }

    /// <summary>Machine-readable name/version of the model that produced <see cref="PredictedFailureLikelihood"/>.</summary>
    public string? PredictionModelVersion { get; init; }

    /// <summary>UTC instant the prediction was generated.</summary>
    public UtcTimestamp? PredictionGeneratedAtUtc { get; init; }
}
