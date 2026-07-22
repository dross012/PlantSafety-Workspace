// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           IActiveMonitoringAuditingDomainEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all ActiveMonitoringAuditing domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Events;

/// <summary>Marker interface for all ActiveMonitoringAuditing domain events.</summary>
public interface IActiveMonitoringAuditingDomainEvent : IDomainEvent { }