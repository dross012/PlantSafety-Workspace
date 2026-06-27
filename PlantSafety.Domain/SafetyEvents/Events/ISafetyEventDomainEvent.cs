// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents
//  Layer:          Domain
//  File:           ISafetyEventDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level SafetyEvents domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Events;

/// <summary>Marker interface for top-level SafetyEvents domain events.</summary>
public interface ISafetyEventDomainEvent : IDomainEvent { }
