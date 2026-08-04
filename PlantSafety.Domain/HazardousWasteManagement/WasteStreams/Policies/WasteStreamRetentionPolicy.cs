// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamRetentionPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing waste-characterization data retention/versioning:
//                  characterization data is versioned and immutable per version, never overwritten.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Policies;

/// <summary>Determines whether a WasteStream aggregate may be re-characterized in place (versioned) versus deactivated.</summary>
public static class WasteStreamRetentionPolicy
{
    /// <summary>Returns <see langword="true"/> when the waste stream may accept a new characterization version.</summary>
    public static bool CanRecharacterize(PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates.WasteStream aggregate)
        => aggregate.Status == WasteStreamStatus.Characterized;

    /// <summary>Returns <see langword="true"/> when prior characterization versions must be preserved (always true; additive-only).</summary>
    public static bool MustPreservePriorVersions() => true;
}
