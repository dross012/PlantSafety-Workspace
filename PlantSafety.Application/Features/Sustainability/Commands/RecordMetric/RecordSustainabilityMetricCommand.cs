// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Command to record a new sustainability metric observation.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.Sustainability.Commands.RecordMetric;

/// <summary>
/// Records a new sustainability metric observation (emissions/energy/water/waste) for a
/// reporting period. Caller must hold <c>Sustainability.Metrics.Record</c>.
/// </summary>
public sealed record RecordSustainabilityMetricCommand : ICommand<Guid>
{
    /// <inheritdoc/>
    public required Guid CommandId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid? CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime RequestedAtUtc { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <summary>Site this metric is scoped to. Null means tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Display name of the metric.</summary>
    public required string MetricName { get; init; }

    /// <summary>Metric category (e.g., Emissions, Energy, Water, Waste).</summary>
    public required string Category { get; init; }

    /// <summary>Measured value for the reporting period.</summary>
    public required double Value { get; init; }

    /// <summary>Unit of measure (e.g., tCO2e, kWh, m3, kg).</summary>
    public required string Unit { get; init; }

    /// <summary>Target value for the reporting period, or null.</summary>
    public double? Target { get; init; }

    /// <summary>UTC timestamp marking the end of the reporting period.</summary>
    public required DateTime PeriodEndUtc { get; init; }
}
