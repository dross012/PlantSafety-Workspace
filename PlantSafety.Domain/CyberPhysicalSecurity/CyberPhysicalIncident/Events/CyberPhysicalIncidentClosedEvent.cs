// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentClosedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a CyberPhysicalIncident is formally closed. Carries ResponseTimeMinutes
//                  to drive the "incident response time" KPI.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Events;

/// <summary>Raised when a <see cref="Aggregates.CyberPhysicalIncident"/> is closed.</summary>
public sealed record CyberPhysicalIncidentClosedEvent(
    CyberPhysicalIncidentEventId EventId,
    CyberPhysicalIncidentId       CyberPhysicalIncidentId,
    int                            ResponseTimeMinutes,
    UtcTimestamp                    ClosedAtUtc,
    PerformedByActor                Actor,
    CorrelationId                    CorrelationId,
    CausationId                      CausationId
) : ICyberPhysicalIncidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CyberPhysicalIncidentClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
