// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for Carrier aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Policies;

/// <summary>Determines whether a <see cref="Carrier"/> aggregate is eligible for archival.</summary>
public static class CarrierArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the carrier record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.LogisticsTransportSafety.Carriers.Aggregates.Carrier aggregate)
        => aggregate.Status == CarrierStatus.Retired;
}
