// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           IEmergencyDrillPlanningDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all EmergencyDrillPlanning domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Events;

/// <summary>Marker interface for all EmergencyDrillPlanning domain events.</summary>
public interface IEmergencyDrillPlanningDomainEvent : IDomainEvent { }