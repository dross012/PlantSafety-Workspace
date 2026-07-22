// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           IStopWorkEventDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all StopWorkEvent domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Events;

/// <summary>Marker interface for all StopWorkEvent domain events.</summary>
public interface IStopWorkEventDomainEvent : IDomainEvent { }