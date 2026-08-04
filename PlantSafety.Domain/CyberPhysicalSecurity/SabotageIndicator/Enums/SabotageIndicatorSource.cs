// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           SabotageIndicatorSource.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Origin subsystem that produced a sabotage indicator, used to determine
//                  cross-source correlation for auto-escalation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;

/// <summary>Origin subsystem that produced a sabotage indicator.</summary>
public enum SabotageIndicatorSource
{
    /// <summary>Derived from an unauthorized ControlChangeEvent.</summary>
    ControlChange = 0,

    /// <summary>Derived from a failed SensorIntegrityCheck.</summary>
    SensorIntegrity = 1,

    /// <summary>Derived from a physical-access anomaly (future PhysicalAccessControl module integration point).</summary>
    PhysicalAccess = 2,

    /// <summary>Derived from an external threat-intelligence feed.</summary>
    ExternalIntel = 3,

    /// <summary>Manually raised by a security analyst.</summary>
    Manual = 4
}
