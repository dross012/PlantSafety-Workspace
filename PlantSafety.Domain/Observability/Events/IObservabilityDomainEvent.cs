// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability
//  Layer:          Domain
//  File:           IObservabilityDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level Observability domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.Events;

/// <summary>Marker interface for top-level Observability domain events.</summary>
public interface IObservabilityDomainEvent : IDomainEvent { }