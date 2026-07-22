// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           IFacilityTrafficMonitoringDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all FacilityTrafficMonitoring domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Events;

/// <summary>Marker interface for all FacilityTrafficMonitoring domain events.</summary>
public interface IFacilityTrafficMonitoringDomainEvent : IDomainEvent { }