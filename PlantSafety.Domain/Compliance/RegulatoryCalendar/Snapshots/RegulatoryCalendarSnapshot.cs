// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendarSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for RegulatoryCalendar aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Snapshots;

/// <summary>Immutable snapshot of a <see cref="RegulatoryCalendar"/> aggregate for fast rehydration.</summary>
public sealed record RegulatoryCalendarSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public RegulatoryCalendarId RegulatoryCalendarId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public RegulatoryCalendarStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
