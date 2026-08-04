// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the Carrier aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Enums;

/// <summary>Lifecycle states for the <see cref="Carrier"/> aggregate root.</summary>
public enum CarrierStatus
{
    /// <summary>Carrier registered and awaiting safety/insurance qualification review.</summary>
    PendingQualification = 0,

    /// <summary>Carrier qualified to perform transport work.</summary>
    Qualified = 1,

    /// <summary>Carrier temporarily suspended from performing transport work.</summary>
    Suspended = 2,

    /// <summary>Carrier permanently retired/deactivated.</summary>
    Retired = 3,
}
