// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           HazardousWasteManifestNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a HazardousWasteManifest aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Exceptions;

/// <summary>Thrown when a HazardousWasteManifest aggregate lookup finds no match.</summary>
public sealed class HazardousWasteManifestNotFoundException : HazardousWasteManifestDomainException
{
    /// <summary>Creates a not-found exception for the given HazardousWasteManifest identity.</summary>
    public HazardousWasteManifestNotFoundException(HazardousWasteManifestId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"HazardousWasteManifest with id '{id}' was not found.",
            "HazardousWasteManifest.NotFound", "NotFound", "Medium", correlationId, causationId)
    {
    }
}
