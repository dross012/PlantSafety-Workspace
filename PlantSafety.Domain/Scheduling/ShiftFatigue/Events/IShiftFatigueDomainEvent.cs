// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.ShiftFatigue
//  Layer:          Domain
//  File:           IShiftFatigueDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ShiftFatigue domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.ShiftFatigue.Events;

/// <summary>Marker interface for all ShiftFatigue domain events.</summary>
public interface IShiftFatigueDomainEvent : IDomainEvent { }