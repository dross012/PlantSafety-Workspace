// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing WasteStream aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Interfaces;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Factories;

/// <summary>Default factory implementation for constructing WasteStream aggregates.</summary>
public sealed class WasteStreamFactory : IWasteStreamFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream Create(
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
        string generatorEpaId)
        => PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId, streamName, epaWasteCode, hazardClass, generatorEpaId);
}
