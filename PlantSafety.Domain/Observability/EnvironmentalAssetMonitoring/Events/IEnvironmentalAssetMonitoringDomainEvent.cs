// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           IEnvironmentalAssetMonitoringDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all EnvironmentalAssetMonitoring domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Events;

/// <summary>Marker interface for all EnvironmentalAssetMonitoring domain events.</summary>
public interface IEnvironmentalAssetMonitoringDomainEvent : IDomainEvent { }