// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           ProceduresSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Procedures aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Procedures"/> aggregate for fast rehydration.</summary>
public sealed record ProceduresSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ProceduresId ProceduresId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ProceduresStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
