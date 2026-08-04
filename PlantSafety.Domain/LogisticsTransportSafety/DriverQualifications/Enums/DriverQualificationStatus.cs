// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the DriverQualification aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Enums;

/// <summary>Lifecycle states for the <see cref="DriverQualification"/> aggregate root.</summary>
public enum DriverQualificationStatus
{
    /// <summary>Qualification submitted and awaiting review.</summary>
    PendingReview = 0,

    /// <summary>Driver is currently qualified.</summary>
    Qualified = 1,

    /// <summary>Qualification temporarily suspended.</summary>
    Suspended = 2,

    /// <summary>Qualification has expired.</summary>
    Expired = 3,

    /// <summary>Qualification permanently retired.</summary>
    Retired = 4,
}
