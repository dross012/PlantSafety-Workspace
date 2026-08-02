// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           IPredictiveMaintenanceAlertDomainEvent.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PredictiveMaintenanceAlert domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PredictiveMaintenance.Events;

/// <summary>Marker interface for all PredictiveMaintenanceAlert domain events.</summary>
public interface IPredictiveMaintenanceAlertDomainEvent : IDomainEvent { }
