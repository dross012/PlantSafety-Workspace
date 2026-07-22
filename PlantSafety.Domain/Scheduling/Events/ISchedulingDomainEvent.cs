// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling
//  Layer:          Domain
//  File:           ISchedulingDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level Scheduling domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.Events;

/// <summary>Marker interface for top-level Scheduling domain events.</summary>
public interface ISchedulingDomainEvent : IDomainEvent { }