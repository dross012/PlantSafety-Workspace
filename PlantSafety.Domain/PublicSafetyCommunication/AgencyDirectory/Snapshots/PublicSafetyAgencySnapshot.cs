// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencySnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PublicSafetyAgency aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PublicSafetyAgency"/> aggregate for fast rehydration.</summary>
public sealed record PublicSafetyAgencySnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PublicSafetyAgencyId PublicSafetyAgencyId { get; init; }

    /// <summary>Operational status at snapshot time.</summary>
    public AgencyOperationalStatus Status { get; init; }

    /// <summary>Verification status at snapshot time.</summary>
    public AgencyVerificationStatus VerificationStatus { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
