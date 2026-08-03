// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.SlaMonitoring
//  Layer:          Domain
//  File:           SlaStatus.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Genuinely new bounded context (UI Gaps Scoping, IncidentManagement Part 2):
//                  no SLA/threshold/escalation-timer concept existed anywhere in the platform
//                  prior to this pass. Classifies an incident's SLA clock relative to its
//                  resolution-time policy target. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.SlaMonitoring.Enums;

/// <summary>Classification of an incident's SLA clock relative to its policy target.</summary>
public enum SlaStatus
{
    /// <summary>Well within the policy target.</summary>
    OnTrack = 0,

    /// <summary>Approaching the policy target (within the at-risk threshold window).</summary>
    AtRisk = 1,

    /// <summary>Past the policy target.</summary>
    Breached = 2,
}
