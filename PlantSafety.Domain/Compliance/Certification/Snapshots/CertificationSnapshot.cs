// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationSnapshot.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Certification aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Certification"/> aggregate for fast rehydration.</summary>
public sealed record CertificationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public CertificationId CertificationId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public CertificationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
