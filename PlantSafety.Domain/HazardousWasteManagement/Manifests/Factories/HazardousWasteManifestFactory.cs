// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           HazardousWasteManifestFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing HazardousWasteManifest aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces;

namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Factories;

/// <summary>Default factory implementation for constructing HazardousWasteManifest aggregates.</summary>
public sealed class HazardousWasteManifestFactory : IHazardousWasteManifestFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string manifestNumber, string generatorEpaId, Guid wasteStreamId)
        => PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, manifestNumber, generatorEpaId, wasteStreamId);
}
