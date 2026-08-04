// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           ICyberPhysicalIncidentDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all CyberPhysicalIncident domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Events;

/// <summary>Marker interface for all CyberPhysicalIncident domain events.</summary>
public interface ICyberPhysicalIncidentDomainEvent : IDomainEvent
{
    /// <summary>Identity of the event instance.</summary>
    CyberPhysicalIncidentEventId EventId { get; }
}
