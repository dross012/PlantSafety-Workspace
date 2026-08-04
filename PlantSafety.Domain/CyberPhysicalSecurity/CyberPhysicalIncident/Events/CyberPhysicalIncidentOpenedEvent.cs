// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentOpenedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new CyberPhysicalIncident is opened.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Events;

/// <summary>Raised when a new <see cref="Aggregates.CyberPhysicalIncident"/> is opened.</summary>
public sealed record CyberPhysicalIncidentOpenedEvent(
    CyberPhysicalIncidentEventId EventId,
    CyberPhysicalIncidentId       CyberPhysicalIncidentId,
    CyberPhysicalIncidentSeverity Severity,
    UtcTimestamp                    OpenedAtUtc,
    PerformedByActor                Actor,
    CorrelationId                    CorrelationId,
    CausationId                      CausationId
) : ICyberPhysicalIncidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CyberPhysicalIncidentOpened";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => OpenedAtUtc;
}
