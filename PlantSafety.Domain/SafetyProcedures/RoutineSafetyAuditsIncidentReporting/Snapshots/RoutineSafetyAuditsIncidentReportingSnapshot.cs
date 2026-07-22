// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for RoutineSafetyAuditsIncidentReporting aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Snapshots;

/// <summary>Immutable snapshot of a <see cref="RoutineSafetyAuditsIncidentReporting"/> aggregate for fast rehydration.</summary>
public sealed record RoutineSafetyAuditsIncidentReportingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public RoutineSafetyAuditsIncidentReportingId RoutineSafetyAuditsIncidentReportingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public RoutineSafetyAuditsIncidentReportingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}