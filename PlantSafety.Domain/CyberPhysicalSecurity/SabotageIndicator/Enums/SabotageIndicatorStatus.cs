// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageIndicatorStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status of a sabotage indicator. Drives the
//                  "sabotage-indicator false-positive rate" KPI (FalsePositive terminal state).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;

/// <summary>Lifecycle status of a sabotage indicator.</summary>
public enum SabotageIndicatorStatus
{
    /// <summary>Newly raised, not yet triaged.</summary>
    Open = 0,

    /// <summary>Actively being investigated.</summary>
    UnderInvestigation = 1,

    /// <summary>Confirmed as a genuine sabotage indication; escalated to CyberPhysicalIncident/RiskManagement.</summary>
    Confirmed = 2,

    /// <summary>Investigated and determined to be a false positive.</summary>
    FalsePositive = 3,

    /// <summary>Closed with no further action.</summary>
    Closed = 4
}
