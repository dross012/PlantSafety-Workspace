// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Training
//  Layer:          Domain
//  File:           ITrainingDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all Training domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Training.Events;

/// <summary>Marker interface for all Training domain events.</summary>
public interface ITrainingDomainEvent : IDomainEvent { }
