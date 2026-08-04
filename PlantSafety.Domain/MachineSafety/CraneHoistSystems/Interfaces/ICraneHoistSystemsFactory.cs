// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           ICraneHoistSystemsFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing CraneHoistSystems aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;

/// <summary>Factory interface for constructing <see cref="CraneHoistSystems"/> aggregates.</summary>
public interface ICraneHoistSystemsFactory
{
    /// <summary>Creates a new CraneHoistSystems aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems Create(
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
        decimal ratedLoadCapacityKg = 0m);
}
