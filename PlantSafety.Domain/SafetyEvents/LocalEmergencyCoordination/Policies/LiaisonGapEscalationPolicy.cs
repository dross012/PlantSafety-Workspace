// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LiaisonGapEscalationPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain policy determining whether an unresolved liaison communication gap
//                  during a live incident must escalate through RiskManagement, per the module's
//                  risk hook. Pure predicate logic only -- actual cross-module escalation
//                  (raising a RiskManagement record) is an Application-layer orchestration
//                  concern that consumes ILocalEmergencyCoordinationDomainEvent /
//                  LiaisonGapDetectedException, keeping this module decoupled from
//                  Operational.RiskManagement at the Domain layer.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Policies;

/// <summary>Determines whether an <see cref="Entities.ExternalResponderLiaison"/> has an unresolved communication gap requiring escalation.</summary>
public static class LiaisonGapEscalationPolicy
{
    /// <summary>Maximum permitted silence period between liaison log entries during a live incident before escalation is required.</summary>
    public static readonly TimeSpan MaxSilenceWindow = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Returns <see langword="true"/> when the elapsed time since the liaison's last logged
    /// communication exceeds <see cref="MaxSilenceWindow"/> and the escalation must be raised
    /// through RiskManagement.
    /// </summary>
    public static bool RequiresEscalation(UtcTimestamp? lastCommunicationAtUtc, UtcTimestamp asOfUtc)
    {
        if (lastCommunicationAtUtc is null)
            return true;

        return asOfUtc.Value - lastCommunicationAtUtc.Value.Value > MaxSilenceWindow;
    }
}
