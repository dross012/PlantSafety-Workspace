// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           IAdvancedMetricsDomainEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Marker interface for all AdvancedMetrics domain events (AdvancedMetricsLedger
//                  and EsgDisclosureReport aggregates share this event contract).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Events;

/// <summary>Marker interface for all AdvancedMetrics domain events.</summary>
public interface IAdvancedMetricsDomainEvent : IDomainEvent { }
