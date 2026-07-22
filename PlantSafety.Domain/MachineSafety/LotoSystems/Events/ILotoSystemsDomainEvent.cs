// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           ILotoSystemsDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all LotoSystems domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Events;

/// <summary>Marker interface for all LotoSystems domain events.</summary>
public interface ILotoSystemsDomainEvent : IDomainEvent { }