// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           IGlobalSafetyInsightsPublisher.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Application-layer abstraction over publishing the internal cross-site
//                  event-mesh's dashboard-facing events (GlobalSafetyKpiSnapshotEvent,
//                  GlobalSafetyAnomalyEvent) whenever a snapshot is computed. Implemented in
//                  Infrastructure as GlobalSafetyInsightsPublisher/GlobalSafetyDashboardPublisher.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;

namespace PlantSafety.Application.GlobalSafetyInsights.Publishers;

/// <summary>Publishes the dashboard-facing GlobalSafetyInsights mesh events for a freshly computed snapshot.</summary>
public interface IGlobalSafetyInsightsPublisher
{
    /// <summary>
    /// Publishes one GlobalSafetyKpiSnapshotEvent and one GlobalSafetyAnomalyEvent per detected
    /// anomaly for the given freshly computed <paramref name="aggregate"/>, respecting data
    /// residency and tenant-region routing.
    /// </summary>
    Task PublishAsync(GlobalSafetyInsightsAggregate aggregate, Guid correlationId, Guid causationId, CancellationToken cancellationToken);
}
