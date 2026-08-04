// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing CraneHoistSystems aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Factories;

/// <summary>Default factory implementation for constructing <see cref="CraneHoistSystems"/> aggregates.</summary>
public sealed class CraneHoistSystemsFactory : ICraneHoistSystemsFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string assetTag = "",
        string assetTitle = "",
        CraneHoistAssetType assetType = CraneHoistAssetType.OverheadBridgeCrane,
        decimal ratedLoadCapacityKg = 0m)
        => PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, assetTag, assetTitle, assetType, ratedLoadCapacityKg);
}
