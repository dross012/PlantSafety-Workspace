// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless lifecycle-transition rules for the CyberPhysicalIncident
//                  aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Rules;

/// <summary>Pure lifecycle-transition rules for <see cref="Aggregates.CyberPhysicalIncident"/>.</summary>
public static class CyberPhysicalIncidentLifecycleRules
{
    /// <summary>Canonical allowed status transition map.</summary>
    public static readonly IReadOnlyDictionary<CyberPhysicalIncidentStatus, CyberPhysicalIncidentStatus[]> AllowedTransitions =
        new Dictionary<CyberPhysicalIncidentStatus, CyberPhysicalIncidentStatus[]>
        {
            [CyberPhysicalIncidentStatus.Open]          = [CyberPhysicalIncidentStatus.Investigating, CyberPhysicalIncidentStatus.Closed],
            [CyberPhysicalIncidentStatus.Investigating] = [CyberPhysicalIncidentStatus.Contained, CyberPhysicalIncidentStatus.Closed],
            [CyberPhysicalIncidentStatus.Contained]     = [CyberPhysicalIncidentStatus.Resolved, CyberPhysicalIncidentStatus.Closed],
            [CyberPhysicalIncidentStatus.Resolved]      = [CyberPhysicalIncidentStatus.Closed],
            [CyberPhysicalIncidentStatus.Closed]        = [],
        };

    /// <summary>Determines whether a transition from <paramref name="from"/> to <paramref name="to"/> is valid.</summary>
    public static bool IsValidTransition(CyberPhysicalIncidentStatus from, CyberPhysicalIncidentStatus to)
        => AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
