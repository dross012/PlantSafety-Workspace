// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           IDigitalTwinDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all DigitalTwin domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Events;

/// <summary>Marker interface for all DigitalTwin domain events.</summary>
public interface IDigitalTwinDomainEvent : IDomainEvent { }
