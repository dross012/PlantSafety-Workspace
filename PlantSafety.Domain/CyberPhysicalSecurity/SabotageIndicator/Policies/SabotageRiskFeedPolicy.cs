// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageRiskFeedPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Policy implementing the module's risk-hook requirement: "sabotage indicators
//                  feed RiskManagement at elevated priority". Maps a SabotageIndicator's severity
//                  to the RiskManagement priority tier that Infrastructure/Application-layer
//                  integration code should apply when publishing into the RiskManagement module.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Policies;

/// <summary>Maps SabotageIndicator severity to a RiskManagement priority tier.</summary>
public static class SabotageRiskFeedPolicy
{
    /// <summary>
    /// Returns the RiskManagement priority code ("P1".."P4") a SabotageIndicator of the given
    /// severity should be filed at. All SabotageIndicators feed RiskManagement at an elevated
    /// priority relative to a routine hazard identification of the same nominal severity, per the
    /// module spec's risk-hook requirement.
    /// </summary>
    public static string ResolveRiskManagementPriority(SabotageIndicatorSeverity severity) => severity switch
    {
        SabotageIndicatorSeverity.Critical => "P1",
        SabotageIndicatorSeverity.High      => "P1",
        SabotageIndicatorSeverity.Medium    => "P2",
        SabotageIndicatorSeverity.Low       => "P3",
        _                                    => "P3"
    };
}
