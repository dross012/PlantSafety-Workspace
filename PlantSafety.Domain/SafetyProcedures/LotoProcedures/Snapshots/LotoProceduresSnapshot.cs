// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for LotoProcedures aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Snapshots;

/// <summary>Immutable snapshot of a <see cref="LotoProcedures"/> aggregate for fast rehydration.</summary>
public sealed record LotoProceduresSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public LotoProceduresId LotoProceduresId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public LotoProceduresStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}