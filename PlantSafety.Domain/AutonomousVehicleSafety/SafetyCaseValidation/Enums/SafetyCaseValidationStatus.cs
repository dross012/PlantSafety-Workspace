// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the SafetyCaseValidation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.SafetyCaseValidation"/> aggregate root.</summary>
public enum SafetyCaseValidationStatus
{
    /// <summary>Safety case submitted and awaiting review.</summary>
    PendingReview = 0,

    /// <summary>Safety case reviewed and validated.</summary>
    Validated = 1,

    /// <summary>Safety case validation has expired and requires revalidation.</summary>
    Expired = 2,

    /// <summary>Safety case was reviewed and rejected.</summary>
    Rejected = 3,
}
