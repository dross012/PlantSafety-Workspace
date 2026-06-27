// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           IWorkerWellBeingDomainEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all WorkerWellBeing domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Events;

/// <summary>Marker interface for all WorkerWellBeing domain events.</summary>
public interface IWorkerWellBeingDomainEvent : IDomainEvent { }
