// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for TransportVehicle aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Policies;

/// <summary>Determines whether a <see cref="TransportVehicle"/> aggregate is eligible for archival.</summary>
public static class TransportVehicleArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the vehicle record may be archived (retired only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Aggregates.TransportVehicle aggregate)
        => aggregate.Status == TransportVehicleStatus.Retired;
}
