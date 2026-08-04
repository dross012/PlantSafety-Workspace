// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           HazardousWasteManifestLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the HazardousWasteManifest aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Rules;

/// <summary>Defines permitted lifecycle transitions for the HazardousWasteManifest aggregate.</summary>
public static class HazardousWasteManifestLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when transporter pickup may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordTransporterPickup(HazardousWasteManifestStatus current) => current == HazardousWasteManifestStatus.Generated;

    /// <summary>Returns <see langword="true"/> when TSDF receipt may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordTsdfReceipt(HazardousWasteManifestStatus current) => current == HazardousWasteManifestStatus.InTransit;

    /// <summary>Returns <see langword="true"/> when the manifest may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(HazardousWasteManifestStatus current) => current == HazardousWasteManifestStatus.ReceivedAtTsdf;

    /// <summary>Returns <see langword="true"/> when a discrepancy may be flagged for <paramref name="current"/> (open manifests only).</summary>
    public static bool CanFlagDiscrepancy(HazardousWasteManifestStatus current) => current != HazardousWasteManifestStatus.Closed;
}
