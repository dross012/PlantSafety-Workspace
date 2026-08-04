// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           IWasteStreamFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing WasteStream aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Interfaces;

/// <summary>Factory interface for constructing WasteStream aggregates.</summary>
public interface IWasteStreamFactory
{
    /// <summary>Characterizes a new WasteStream aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string streamName,
        string epaWasteCode,
        WasteHazardClass hazardClass,
        string generatorEpaId);
}
