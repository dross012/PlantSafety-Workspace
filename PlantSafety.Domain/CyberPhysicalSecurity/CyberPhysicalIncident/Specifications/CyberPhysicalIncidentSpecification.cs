// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Base specification abstraction for querying CyberPhysicalIncident aggregates
//                  in-memory, plus the "active incidents" specification backing the Active
//                  Sabotage Indicator Board / incident-response dashboards.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Specifications;

/// <summary>Base specification for querying <see cref="Aggregates.CyberPhysicalIncident"/> aggregates.</summary>
public abstract class CyberPhysicalIncidentSpecification
{
    /// <summary>Converts this specification into an in-memory predicate.</summary>
    public abstract Func<Aggregates.CyberPhysicalIncident, bool> ToPredicate();
}

/// <summary>Matches CyberPhysicalIncidents that are not yet Closed.</summary>
public sealed class ActiveCyberPhysicalIncidentsSpecification : CyberPhysicalIncidentSpecification
{
    /// <inheritdoc/>
    public override Func<Aggregates.CyberPhysicalIncident, bool> ToPredicate()
        => i => i.Status != CyberPhysicalIncidentStatus.Closed;
}

/// <summary>Matches CyberPhysicalIncidents at or above the given severity.</summary>
public sealed class CyberPhysicalIncidentsByMinimumSeveritySpecification(CyberPhysicalIncidentSeverity minimumSeverity) : CyberPhysicalIncidentSpecification
{
    /// <inheritdoc/>
    public override Func<Aggregates.CyberPhysicalIncident, bool> ToPredicate()
        => i => i.Severity >= minimumSeverity;
}
