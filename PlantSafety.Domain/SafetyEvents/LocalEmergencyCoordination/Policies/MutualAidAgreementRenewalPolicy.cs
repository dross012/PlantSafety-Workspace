// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           MutualAidAgreementRenewalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy computing mutual-aid agreement renewal/currency status, backing
//                  the "agreement currency (renewal status)" KPI and the periodic mutual-aid
//                  agreement review/renewal scheduling hook.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies;

/// <summary>Computes renewal/currency status for a <see cref="Entities.MutualAidAgreement"/>.</summary>
public static class MutualAidAgreementRenewalPolicy
{
    /// <summary>Number of days before <c>RenewalDueAtUtc</c> that an agreement enters the review window.</summary>
    public const int ReviewWindowDays = 60;

    /// <summary>Evaluates the renewal status of an agreement given its due date and the current instant.</summary>
    public static MutualAidAgreementRenewalStatus Evaluate(UtcTimestamp renewalDueAtUtc, UtcTimestamp asOfUtc)
    {
        if (asOfUtc.Value >= renewalDueAtUtc.Value)
            return MutualAidAgreementRenewalStatus.Expired;

        if (asOfUtc.Value >= renewalDueAtUtc.Value.AddDays(-ReviewWindowDays))
            return MutualAidAgreementRenewalStatus.PendingRenewal;

        return MutualAidAgreementRenewalStatus.Current;
    }
}
