// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the DriverQualification aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="DriverQualification"/> aggregate.</summary>
public static class DriverQualificationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when a qualification may be suspended from <paramref name="current"/>.</summary>
    public static bool CanSuspend(DriverQualificationStatus current) => current == DriverQualificationStatus.Qualified;

    /// <summary>Returns <see langword="true"/> when a qualification may be reinstated from <paramref name="current"/>.</summary>
    public static bool CanReinstate(DriverQualificationStatus current) => current == DriverQualificationStatus.Suspended;

    /// <summary>Returns <see langword="true"/> when the qualification may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(DriverQualificationStatus current) => current != DriverQualificationStatus.Retired;
}
