// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           MutualAidAgreementRenewalStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Renewal/currency status for a MutualAidAgreement child entity, backing the
//                  "agreement currency (renewal status)" KPI and the periodic mutual-aid
//                  agreement review/renewal scheduling hook.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Enums;

/// <summary>Renewal/currency status for a <see cref="Entities.MutualAidAgreement"/>.</summary>
public enum MutualAidAgreementRenewalStatus
{
    /// <summary>Agreement is current and not yet due for review.</summary>
    Current = 0,

    /// <summary>Agreement is within its review/renewal window (Handbook default: 60 days before <c>RenewalDueAtUtc</c>).</summary>
    PendingRenewal = 1,

    /// <summary>Agreement renewal date has passed without a recorded renewal.</summary>
    Expired = 2,
}
