// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           ManifestImmutableException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Thrown when a mutation is attempted against a manifest that has already been
//                  transporter-signed and is therefore immutable per the data-governance
//                  requirement (manifest records immutable once transporter-signed).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Exceptions;

/// <summary>Thrown when a mutation is attempted against an immutable, transporter-signed manifest.</summary>
public sealed class ManifestImmutableException : HazardousWasteManifestDomainException
{
    /// <summary>Creates an immutability-violation exception for the given manifest identity.</summary>
    public ManifestImmutableException(HazardousWasteManifestId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"HazardousWasteManifest {id} is immutable once transporter-signed and cannot be modified.",
            "HazardousWasteManifest.Immutable", "Governance", "High", correlationId, causationId)
    {
    }
}
