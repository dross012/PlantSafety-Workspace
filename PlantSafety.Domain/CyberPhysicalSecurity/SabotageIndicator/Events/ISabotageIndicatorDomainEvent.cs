// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SabotageIndicator
//  Layer:          Domain
//  File:           ISabotageIndicatorDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all SabotageIndicator domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Events;

/// <summary>Marker interface for all SabotageIndicator domain events.</summary>
public interface ISabotageIndicatorDomainEvent : IDomainEvent
{
    /// <summary>Identity of the event instance.</summary>
    SabotageIndicatorEventId EventId { get; }
}
