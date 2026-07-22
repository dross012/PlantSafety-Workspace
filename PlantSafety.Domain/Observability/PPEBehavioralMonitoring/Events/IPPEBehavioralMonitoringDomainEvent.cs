// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           IPPEBehavioralMonitoringDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all PPEBehavioralMonitoring domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Events;

/// <summary>Marker interface for all PPEBehavioralMonitoring domain events.</summary>
public interface IPPEBehavioralMonitoringDomainEvent : IDomainEvent { }