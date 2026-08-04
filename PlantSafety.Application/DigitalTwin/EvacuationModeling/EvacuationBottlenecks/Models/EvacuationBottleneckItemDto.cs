// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single EvacuationBottleneck record
//                    ("DigitalTwin.EvacuationModeling.EvacuationBottlenecks").
// SchemaVersion: 1

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Models;

/// <summary>A single EvacuationBottleneck record.</summary>
public sealed record EvacuationBottleneckItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EvacuationBottleneckId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Identity of the evacuation route the bottleneck was detected on.</summary>
    public required Guid RouteId { get; init; }

    /// <summary>Identity of the egress-simulation run that surfaced this bottleneck.</summary>
    public required Guid SimulationRunId { get; init; }

    /// <summary>Free-text description of the bottleneck location within the route.</summary>
    public required string BottleneckLocation { get; init; }

    /// <summary>Delay, in seconds, above the route's estimated egress time observed by the simulation.</summary>
    public required int DelaySeconds { get; init; }

    /// <summary>Number of times this bottleneck has been detected again on subsequent simulation runs.</summary>
    public required int RecurrenceCount { get; init; }

    /// <summary>Overall status classification: Detected, Recurred, Mitigated.</summary>
    public required string BottleneckStatus { get; init; }

    /// <summary>UTC timestamp the bottleneck was first detected.</summary>
    public required DateTime DetectedAtUtc { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
