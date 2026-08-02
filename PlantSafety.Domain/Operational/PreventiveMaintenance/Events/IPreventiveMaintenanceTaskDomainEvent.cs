// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           IPreventiveMaintenanceTaskDomainEvent.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PreventiveMaintenanceTask domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PreventiveMaintenance.Events;

/// <summary>Marker interface for all PreventiveMaintenanceTask domain events.</summary>
public interface IPreventiveMaintenanceTaskDomainEvent : IDomainEvent { }
