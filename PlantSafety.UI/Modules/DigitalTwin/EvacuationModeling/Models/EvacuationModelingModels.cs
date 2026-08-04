// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the DigitalTwin.EvacuationModeling module, mirroring
//   the PlantSafety.Api read-model shapes for EvacuationRoutes, EvacuationZones,
//   EgressSimulations, and EvacuationBottlenecks.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.DigitalTwin.EvacuationModeling.Models;

/// <summary>A single evacuation route read-model item.</summary>
public sealed class EvacuationRouteDto
{
    /// <summary>Gets the route identifier.</summary>
    public Guid EvacuationRouteId { get; init; }

    /// <summary>Gets the route name.</summary>
    public string RouteName { get; init; } = string.Empty;

    /// <summary>Gets the identity of the zone this route originates from.</summary>
    public Guid ZoneId { get; init; }

    /// <summary>Gets the estimated egress time under nominal conditions, in seconds.</summary>
    public int EstimatedEgressTimeSeconds { get; init; }

    /// <summary>Gets the cumulative count of obstruction events recorded against this route.</summary>
    public int ObstructionCount { get; init; }

    /// <summary>Gets the current lifecycle status: Active, Obstructed, Retired.</summary>
    public string RouteStatus { get; init; } = string.Empty;
}

/// <summary>A single open (uncleared) route obstruction.</summary>
public sealed class RouteObstructionDto
{
    /// <summary>Gets the obstruction identifier.</summary>
    public Guid RouteObstructionId { get; init; }

    /// <summary>Gets the identity of the parent evacuation route.</summary>
    public Guid EvacuationRouteId { get; init; }

    /// <summary>Gets the obstruction description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the severity classification: Minor, Moderate, Severe.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the obstruction was recorded.</summary>
    public DateTime RecordedAtUtc { get; init; }
}

/// <summary>A single evacuation bottleneck read-model item.</summary>
public sealed class EvacuationBottleneckDto
{
    /// <summary>Gets the bottleneck identifier.</summary>
    public Guid EvacuationBottleneckId { get; init; }

    /// <summary>Gets the identity of the route the bottleneck was detected on.</summary>
    public Guid RouteId { get; init; }

    /// <summary>Gets the bottleneck location description.</summary>
    public string BottleneckLocation { get; init; } = string.Empty;

    /// <summary>Gets the delay, in seconds, observed above the route's estimated egress time.</summary>
    public int DelaySeconds { get; init; }

    /// <summary>Gets the number of times this bottleneck has recurred.</summary>
    public int RecurrenceCount { get; init; }

    /// <summary>Gets the current status: Detected, Recurred, Mitigated.</summary>
    public string BottleneckStatus { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the bottleneck was first detected.</summary>
    public DateTime DetectedAtUtc { get; init; }
}

/// <summary>A single egress-simulation run read-model item.</summary>
public sealed class EgressSimulationDto
{
    /// <summary>Gets the simulation run identifier.</summary>
    public Guid EgressSimulationRunId { get; init; }

    /// <summary>Gets the identity of the zone this run models.</summary>
    public Guid ZoneId { get; init; }

    /// <summary>Gets the current status: Running, Completed, Failed.</summary>
    public string SimulationStatus { get; init; } = string.Empty;

    /// <summary>Gets the simulated total evacuation time in seconds, once completed.</summary>
    public int? SimulatedEvacuationTimeSeconds { get; init; }

    /// <summary>Gets the UTC timestamp the run started.</summary>
    public DateTime StartedAtUtc { get; init; }
}

/// <summary>Envelope for a list-shaped EvacuationModeling read-model endpoint.</summary>
/// <typeparam name="TItem">The item DTO type.</typeparam>
public sealed class EvacuationModelingListResponseDto<TItem>
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the matching items.</summary>
    public IReadOnlyList<TItem> Items { get; init; } = [];
}
