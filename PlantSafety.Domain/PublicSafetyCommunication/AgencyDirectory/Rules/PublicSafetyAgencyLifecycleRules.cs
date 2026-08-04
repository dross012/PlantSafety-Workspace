// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the PublicSafetyAgency aggregate,
//                  including the scheduled verification/refresh cadence used by the periodic
//                  agency-contact verification job.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PublicSafetyAgency"/> aggregate.</summary>
public static class PublicSafetyAgencyLifecycleRules
{
    /// <summary>Default cadence, in days, between scheduled agency-contact verifications.</summary>
    public const int DefaultVerificationCadenceDays = 90;

    /// <summary>Returns <see langword="true"/> when verification may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordVerification(AgencyOperationalStatus current) => current == AgencyOperationalStatus.Active;

    /// <summary>Returns <see langword="true"/> when the agency may be deactivated from <paramref name="current"/>.</summary>
    public static bool CanDeactivate(AgencyOperationalStatus current) => current != AgencyOperationalStatus.Inactive;

    /// <summary>Returns <see langword="true"/> when re-verification is overdue given the current UTC instant.</summary>
    public static bool IsVerificationOverdue(UtcTimestamp? nextVerificationDueAtUtc, UtcTimestamp nowUtc)
        => nextVerificationDueAtUtc is not null && nextVerificationDueAtUtc.Value.Value < nowUtc.Value;
}
