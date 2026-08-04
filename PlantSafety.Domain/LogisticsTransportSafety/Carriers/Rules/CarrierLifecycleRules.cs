// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the Carrier aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Carrier"/> aggregate.</summary>
public static class CarrierLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a carrier may be qualified from <paramref name="current"/>.</summary>
    public static bool CanQualify(CarrierStatus current) =>
        current is CarrierStatus.PendingQualification or CarrierStatus.Suspended;

    /// <summary>Returns <see langword="true"/> when a carrier may be suspended from <paramref name="current"/>.</summary>
    public static bool CanSuspend(CarrierStatus current) => current == CarrierStatus.Qualified;

    /// <summary>Returns <see langword="true"/> when the carrier may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(CarrierStatus current) => current != CarrierStatus.Retired;
}
