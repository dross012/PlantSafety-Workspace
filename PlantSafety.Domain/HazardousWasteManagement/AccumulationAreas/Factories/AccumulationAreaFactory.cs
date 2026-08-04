// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing AccumulationArea aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Interfaces;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Factories;

/// <summary>Default factory implementation for constructing AccumulationArea aggregates.</summary>
public sealed class AccumulationAreaFactory : IAccumulationAreaFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string areaName, string locationDescription, int timeLimitDays)
        => PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, areaName, locationDescription, timeLimitDays);
}
