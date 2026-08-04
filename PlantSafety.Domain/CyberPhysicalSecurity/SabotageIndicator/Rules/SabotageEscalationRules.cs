// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageEscalationRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless rules for the module's risk-hook requirement: "correlated
//                  multi-source indicators auto-escalate". Consumed by
//                  SabotageCorrelationService (Domain.Services).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Rules;

/// <summary>Pure rules governing multi-source sabotage-indicator auto-escalation.</summary>
public static class SabotageEscalationRules
{
    /// <summary>Minimum number of distinct-source indicators within the correlation window required to auto-escalate.</summary>
    public const int AutoEscalationDistinctSourceThreshold = 2;

    /// <summary>Correlation window, in minutes, within which indicators are considered related.</summary>
    public const int CorrelationWindowMinutes = 60;

    /// <summary>
    /// Determines whether a set of correlated indicator sources should trigger auto-escalation:
    /// true when indicators from at least <see cref="AutoEscalationDistinctSourceThreshold"/>
    /// distinct sources fall within the correlation window.
    /// </summary>
    public static bool ShouldAutoEscalate(IReadOnlyCollection<SabotageIndicatorSource> correlatedSources)
        => correlatedSources.Distinct().Count() >= AutoEscalationDistinctSourceThreshold;
}
