// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogRetentionPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy defining the retention period for CommunicationLogEntry records,
//                  per the module's data-governance requirement that communication logs are
//                  retained per the incident-record retention policy as compliance evidence.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Policies;

/// <summary>Defines the retention period for <see cref="CommunicationLogEntry"/> records.</summary>
public static class CommunicationLogRetentionPolicy
{
    /// <summary>Minimum retention period, in years, aligned to the incident-record retention policy.</summary>
    public const int MinimumRetentionYears = 7;

    /// <summary>Returns <see langword="true"/> when a record created at <paramref name="createdAtUtc"/> may be purged as of <paramref name="nowUtc"/>.</summary>
    public static bool IsEligibleForPurge(UtcTimestamp createdAtUtc, UtcTimestamp nowUtc)
        => nowUtc.Value >= createdAtUtc.Value.AddYears(MinimumRetentionYears);
}
