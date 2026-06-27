// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Domain
//  File:           IOperationalSafetyDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all OperationalSafety domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.OperationalSafety.Events;

/// <summary>Marker interface for all OperationalSafety domain events.</summary>
public interface IOperationalSafetyDomainEvent : IDomainEvent { }
