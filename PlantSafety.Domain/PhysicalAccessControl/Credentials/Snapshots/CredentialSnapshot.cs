// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for Credential aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Credential"/> aggregate for fast rehydration.</summary>
public sealed record CredentialSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public CredentialId CredentialId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public CredentialStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
