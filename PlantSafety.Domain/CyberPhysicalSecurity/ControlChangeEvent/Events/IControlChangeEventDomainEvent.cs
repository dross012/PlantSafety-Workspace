// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           IControlChangeEventDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ControlChangeEvent domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Events;

/// <summary>Marker interface for all ControlChangeEvent domain events.</summary>
public interface IControlChangeEventDomainEvent : IDomainEvent
{
    /// <summary>Identity of the event instance.</summary>
    ControlChangeDomainEventId EventId { get; }
}
