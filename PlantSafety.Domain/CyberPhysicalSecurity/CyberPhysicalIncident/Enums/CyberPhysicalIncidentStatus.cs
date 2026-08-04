// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle status of a CyberPhysicalIncident, aligned with the Incident Response
//                  subdomain and NIST CSF/IEC 62443 incident-response phases.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;

/// <summary>Lifecycle status of a CyberPhysicalIncident.</summary>
public enum CyberPhysicalIncidentStatus
{
    /// <summary>Newly opened, response not yet underway.</summary>
    Open = 0,

    /// <summary>Actively being investigated.</summary>
    Investigating = 1,

    /// <summary>Threat contained; root cause identification/remediation in progress.</summary>
    Contained = 2,

    /// <summary>Remediated; pending formal closure.</summary>
    Resolved = 3,

    /// <summary>Formally closed.</summary>
    Closed = 4
}
