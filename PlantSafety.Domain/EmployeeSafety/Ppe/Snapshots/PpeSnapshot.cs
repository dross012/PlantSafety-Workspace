// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           PpeSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Ppe aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Ppe"/> aggregate for fast rehydration.</summary>
public sealed record PpeSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PpeId PpeId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PpeStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}