// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           IEnvironmentalHealthDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all EnvironmentalHealth domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Events;

/// <summary>Marker interface for all EnvironmentalHealth domain events.</summary>
public interface IEnvironmentalHealthDomainEvent : IDomainEvent { }
