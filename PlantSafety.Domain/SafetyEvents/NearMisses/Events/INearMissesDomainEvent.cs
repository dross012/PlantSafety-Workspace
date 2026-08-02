// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Domain
//  File:           INearMissesDomainEvent.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all NearMisses domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.NearMisses.Events;

/// <summary>Marker interface for all NearMisses domain events.</summary>
public interface INearMissesDomainEvent : IDomainEvent { }
