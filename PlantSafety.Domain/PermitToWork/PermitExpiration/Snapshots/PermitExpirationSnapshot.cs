// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitExpiration
//  Layer:          Domain
//  File:           PermitExpirationSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PermitExpiration aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitExpiration.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PermitExpiration"/> aggregate for fast rehydration.</summary>
public sealed record PermitExpirationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PermitExpirationId PermitExpirationId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PermitExpirationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}