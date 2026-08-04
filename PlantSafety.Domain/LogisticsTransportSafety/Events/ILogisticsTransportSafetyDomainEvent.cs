// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety
//  Layer:          Domain
//  File:           ILogisticsTransportSafetyDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for top-level LogisticsTransportSafety domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Events;

/// <summary>Marker interface for top-level LogisticsTransportSafety domain events.</summary>
public interface ILogisticsTransportSafetyDomainEvent : IDomainEvent { }
