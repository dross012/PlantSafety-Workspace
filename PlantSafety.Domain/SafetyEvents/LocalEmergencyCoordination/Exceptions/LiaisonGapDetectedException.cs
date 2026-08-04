// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LiaisonGapDetectedException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown by LiaisonGapEscalationPolicy callers when an unresolved liaison
//                  communication gap is detected during a live incident, per the module's
//                  RiskManagement escalation hook.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Exceptions;

/// <summary>Thrown when an unresolved liaison communication gap during a live incident must escalate through RiskManagement.</summary>
public sealed class LiaisonGapDetectedException : LocalEmergencyCoordinationDomainException
{
    /// <summary>Creates a liaison-gap exception for the given liaison assignment.</summary>
    public LiaisonGapDetectedException(
        ExternalResponderLiaisonId liaisonId, TimeSpan gapDuration,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"ExternalResponderLiaison {liaisonId} has an unresolved communication gap of {gapDuration.TotalMinutes:N0} minutes and must escalate through RiskManagement.",
            "LocalEmergencyCoordination.LiaisonGapDetected",
            "Escalation",
            "High",
            correlationId,
            causationId)
    {
    }
}
