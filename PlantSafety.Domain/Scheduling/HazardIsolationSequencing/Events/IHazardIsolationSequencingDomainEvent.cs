// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           IHazardIsolationSequencingDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all HazardIsolationSequencing domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Events;

/// <summary>Marker interface for all HazardIsolationSequencing domain events.</summary>
public interface IHazardIsolationSequencingDomainEvent : IDomainEvent { }