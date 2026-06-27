// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Domain
//  File:           ContractorSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Contractor aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Contractor.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Contractor"/> aggregate for fast rehydration.</summary>
public sealed record ContractorSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ContractorId ContractorId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ContractorStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
