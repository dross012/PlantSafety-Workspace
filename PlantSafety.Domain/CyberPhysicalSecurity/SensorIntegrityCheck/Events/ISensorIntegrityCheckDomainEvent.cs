// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           ISensorIntegrityCheckDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all SensorIntegrityCheck domain events.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Events;

/// <summary>Marker interface for all SensorIntegrityCheck domain events.</summary>
public interface ISensorIntegrityCheckDomainEvent : IDomainEvent
{
    /// <summary>Identity of the event instance.</summary>
    SensorIntegrityCheckEventId EventId { get; }
}
