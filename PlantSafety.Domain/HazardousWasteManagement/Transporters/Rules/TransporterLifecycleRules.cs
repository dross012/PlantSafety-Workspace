// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Transporters
//  Layer:          Domain
//  File:           TransporterLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the Transporter aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Transporters.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.Transporters.Rules;

/// <summary>Defines permitted lifecycle transitions for the Transporter aggregate.</summary>
public static class TransporterLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the transporter may record a pickup from <paramref name="current"/>.</summary>
    public static bool CanRecordPickup(TransporterStatus current) => current == TransporterStatus.Active;

    /// <summary>Returns <see langword="true"/> when the transporter may be suspended from <paramref name="current"/>.</summary>
    public static bool CanSuspend(TransporterStatus current) => current == TransporterStatus.Active;
}
