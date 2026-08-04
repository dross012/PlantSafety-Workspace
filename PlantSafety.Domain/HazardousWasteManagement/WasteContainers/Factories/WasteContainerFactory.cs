// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing WasteContainer aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Interfaces;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Factories;

/// <summary>Default factory implementation for constructing WasteContainer aggregates.</summary>
public sealed class WasteContainerFactory : IWasteContainerFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, Guid accumulationAreaId, Guid wasteStreamId,
        string containerLabel, decimal capacityGallons, int regulatoryTimeLimitDays)
        => PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId, accumulationAreaId, wasteStreamId, containerLabel, capacityGallons, regulatoryTimeLimitDays);
}
