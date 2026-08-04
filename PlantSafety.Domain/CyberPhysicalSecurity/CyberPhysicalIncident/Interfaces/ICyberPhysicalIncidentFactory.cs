// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           ICyberPhysicalIncidentFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory abstraction for opening a CyberPhysicalIncident from one or more
//                  escalated SabotageIndicators.
//                  Additive-only evolution.
// ============================================================================
using SabotageIndicatorAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces;

/// <summary>Factory abstraction for opening a CyberPhysicalIncident from escalated SabotageIndicators.</summary>
public interface ICyberPhysicalIncidentFactory
{
    /// <summary>Opens a new CyberPhysicalIncident summarizing the given escalated indicators.</summary>
    Aggregates.CyberPhysicalIncident CreateFromEscalatedIndicators(
        Guid tenantId,
        Guid siteId,
        UserId ownerId,
        UserId createdBy,
        IReadOnlyCollection<SabotageIndicatorAggregate> escalatedIndicators);
}
