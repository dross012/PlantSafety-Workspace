// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageCorrelationService.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service implementing the Sabotage Indicator Correlation subdomain:
//                  correlates a newly raised indicator against other active indicators within the
//                  correlation window and determines whether auto-escalation applies.
//                  Pure/stateless -- callers supply the candidate indicator set (typically read via
//                  ISabotageIndicatorRepository.ListForCorrelationWindowAsync).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Rules;
using SabotageIndicatorAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Services;

/// <summary>Correlates sabotage indicators across sources to determine auto-escalation.</summary>
public static class SabotageCorrelationService
{
    /// <summary>
    /// Correlates <paramref name="newIndicator"/> against <paramref name="candidateIndicators"/>
    /// (other active indicators already known to fall within the correlation window) and returns
    /// the correlated count plus whether auto-escalation applies.
    /// </summary>
    public static (int CorrelatedCount, bool AutoEscalate) Correlate(
        SabotageIndicatorAggregate newIndicator,
        IReadOnlyCollection<SabotageIndicatorAggregate> candidateIndicators)
    {
        var sources = new List<SabotageIndicatorSource> { newIndicator.Source };
        sources.AddRange(candidateIndicators.Select(i => i.Source));

        var autoEscalate = SabotageEscalationRules.ShouldAutoEscalate(sources);

        return (candidateIndicators.Count + 1, autoEscalate);
    }
}
