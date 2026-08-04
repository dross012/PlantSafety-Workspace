// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           AgencyVerificationStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Verification/currency status of a PublicSafetyAgency's contact data. Drives
//                  the scheduled agency-contact verification/refresh hook described in the
//                  module data-governance spec.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Enums;

/// <summary>Verification/currency status of a <see cref="PublicSafetyAgency"/>'s contact data.</summary>
public enum AgencyVerificationStatus
{
    /// <summary>Contact data has never been verified.</summary>
    Unverified = 0,

    /// <summary>Contact data was verified within the required cadence.</summary>
    Verified = 1,

    /// <summary>Scheduled re-verification is past due.</summary>
    VerificationOverdue = 2,

    /// <summary>Most recent verification attempt failed (unreachable/invalid contact).</summary>
    VerificationFailed = 3,
}
