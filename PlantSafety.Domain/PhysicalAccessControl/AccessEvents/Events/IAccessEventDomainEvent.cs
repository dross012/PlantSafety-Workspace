// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           IAccessEventDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all AccessEvent domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Events;

/// <summary>Marker interface for all AccessEvent domain events.</summary>
public interface IAccessEventDomainEvent : IDomainEvent { }
