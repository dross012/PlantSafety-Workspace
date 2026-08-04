// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           IHazardousWasteManifestFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing HazardousWasteManifest aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Interfaces;

/// <summary>Factory interface for constructing HazardousWasteManifest aggregates.</summary>
public interface IHazardousWasteManifestFactory
{
    /// <summary>Generates a new HazardousWasteManifest aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.HazardousWasteManagement.Manifests.Aggregates.HazardousWasteManifest Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string manifestNumber, string generatorEpaId, Guid wasteStreamId);
}
