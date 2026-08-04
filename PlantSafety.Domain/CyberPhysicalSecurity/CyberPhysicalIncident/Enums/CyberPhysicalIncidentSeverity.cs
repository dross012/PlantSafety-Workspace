// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentSeverity.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Severity rating of a cyber-physical security incident.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;

/// <summary>Severity rating of a cyber-physical security incident.</summary>
public enum CyberPhysicalIncidentSeverity
{
    /// <summary>Low severity.</summary>
    Low = 0,

    /// <summary>Medium severity.</summary>
    Medium = 1,

    /// <summary>High severity.</summary>
    High = 2,

    /// <summary>Critical severity; imminent or realized physical-safety impact.</summary>
    Critical = 3
}
