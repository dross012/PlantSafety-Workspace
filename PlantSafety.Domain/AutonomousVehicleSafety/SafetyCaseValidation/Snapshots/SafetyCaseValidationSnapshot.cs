// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.SafetyCaseValidation
//  Layer:          Domain
//  File:           SafetyCaseValidationSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for SafetyCaseValidation aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.SafetyCaseValidation.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Aggregates.SafetyCaseValidation"/> aggregate for fast rehydration.</summary>
public sealed record SafetyCaseValidationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public SafetyCaseValidationId SafetyCaseValidationId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public SafetyCaseValidationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
