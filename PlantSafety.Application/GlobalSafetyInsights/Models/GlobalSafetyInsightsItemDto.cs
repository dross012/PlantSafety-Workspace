// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GlobalSafetyInsightsItemDto.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO projecting a computed GlobalSafetyInsightsAggregate snapshot for
//                  the Phase 8 dashboard (built by a later agent) and any other Application-layer
//                  consumer. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.GlobalSafetyInsights.Models;

/// <summary>Read-model projection of a single computed GlobalSafetyInsights snapshot.</summary>
public sealed record GlobalSafetyInsightsItemDto
{
    /// <summary>Snapshot identifier.</summary>
    public required Guid GlobalSafetyInsightsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Region code this snapshot was computed for.</summary>
    public required string RegionCode { get; init; }

    /// <summary>Sites covered by this snapshot.</summary>
    public required IReadOnlyList<Guid> SiteIds { get; init; }

    /// <summary>Inclusive UTC start of the analysis window.</summary>
    public required DateTime WindowStartUtc { get; init; }

    /// <summary>Inclusive UTC end of the analysis window.</summary>
    public required DateTime WindowEndUtc { get; init; }

    /// <summary>Total Recordable Incident Rate.</summary>
    public required decimal Trir { get; init; }

    /// <summary>Lost Time Incident Rate.</summary>
    public required decimal Ltir { get; init; }

    /// <summary>Mean worker exposure-level score across the window.</summary>
    public required decimal ExposureRate { get; init; }

    /// <summary>Incidents per site per day across the window.</summary>
    public required decimal IncidentDensity { get; init; }

    /// <summary>Pearson correlation between degraded machine-state observations and incidents, per site.</summary>
    public required double MachineStateIncidentCorrelation { get; init; }

    /// <summary>Number of anomalies detected in this snapshot.</summary>
    public required int AnomalyCount { get; init; }

    /// <summary>Number of structured recommendations surfaced with this snapshot.</summary>
    public required int RecommendationCount { get; init; }

    /// <summary>UTC instant this snapshot was computed.</summary>
    public required DateTime ComputedAtUtc { get; init; }
}
