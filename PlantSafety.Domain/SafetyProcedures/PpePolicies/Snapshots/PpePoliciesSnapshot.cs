// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PpePolicies aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PpePolicies"/> aggregate for fast rehydration.</summary>
public sealed record PpePoliciesSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PpePoliciesId PpePoliciesId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PpePoliciesStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}