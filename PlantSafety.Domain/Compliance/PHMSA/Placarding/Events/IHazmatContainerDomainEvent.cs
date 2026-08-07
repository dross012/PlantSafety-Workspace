// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           IHazmatContainerDomainEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all HazmatContainer domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Events;

/// <summary>Marker interface for all HazmatContainer domain events.</summary>
public interface IHazmatContainerDomainEvent : IDomainEvent { }
