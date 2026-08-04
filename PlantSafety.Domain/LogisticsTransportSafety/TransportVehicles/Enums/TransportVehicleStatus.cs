// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportVehicles
//  Layer:          Domain
//  File:           TransportVehicleStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the TransportVehicle aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportVehicles.Enums;

/// <summary>Lifecycle states for the <see cref="TransportVehicle"/> aggregate root.</summary>
public enum TransportVehicleStatus
{
    /// <summary>Vehicle registered and awaiting first inspection.</summary>
    Registered = 0,

    /// <summary>Next safety inspection scheduled.</summary>
    InspectionScheduled = 1,

    /// <summary>Most recent inspection passed.</summary>
    InspectionPassed = 2,

    /// <summary>Most recent inspection failed.</summary>
    InspectionFailed = 3,

    /// <summary>Vehicle removed from service pending repair or review.</summary>
    OutOfService = 4,

    /// <summary>Vehicle permanently retired/decommissioned.</summary>
    Retired = 5,
}
