// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           IAccumulationAreaFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing AccumulationArea aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Interfaces;

/// <summary>Factory interface for constructing AccumulationArea aggregates.</summary>
public interface IAccumulationAreaFactory
{
    /// <summary>Registers a new AccumulationArea aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string areaName, string locationDescription, int timeLimitDays);
}
