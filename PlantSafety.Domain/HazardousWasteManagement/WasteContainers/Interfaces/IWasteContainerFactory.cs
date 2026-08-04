// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           IWasteContainerFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing WasteContainer aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Interfaces;

/// <summary>Factory interface for constructing WasteContainer aggregates.</summary>
public interface IWasteContainerFactory
{
    /// <summary>Opens a new WasteContainer aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, Guid accumulationAreaId, Guid wasteStreamId,
        string containerLabel, decimal capacityGallons, int regulatoryTimeLimitDays);
}
