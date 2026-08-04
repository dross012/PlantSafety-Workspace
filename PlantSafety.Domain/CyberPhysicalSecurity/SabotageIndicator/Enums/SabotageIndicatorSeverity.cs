// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageIndicatorSeverity.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Severity rating of a sabotage indicator.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;

/// <summary>Severity rating of a sabotage indicator.</summary>
public enum SabotageIndicatorSeverity
{
    /// <summary>Low severity.</summary>
    Low = 0,

    /// <summary>Medium severity.</summary>
    Medium = 1,

    /// <summary>High severity.</summary>
    High = 2,

    /// <summary>Critical severity; direct physical-safety implication.</summary>
    Critical = 3
}
