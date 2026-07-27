// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-27
// FILE DESCRIPTION: Domain-specific metrics instruments for the Incident Management workflow.
//   Uses the .NET Meter API (System.Diagnostics.Metrics), following the same instrumentation
//   technology already established by PlantSafety.Application.Behaviors.Metrics.MetricsBehavior
//   (the only pre-existing metrics infrastructure found anywhere in this codebase: no
//   Application Insights custom metrics and no OpenTelemetry Meter/exporter wiring exist).
//   Only the incident-creation counter is wired up today (in CreateIncidentCommandHandler).
//   Incident resolution-time (histogram) and open-incident backlog (observable gauge) are
//   intentionally NOT added in this pass: no Close/Resolve Incident command or open-incident
//   count query exists yet in PlantSafety.Application to source real data from, and declaring
//   instruments that are never recorded would create dead/no-op telemetry. Add those instruments
//   in a follow-up once the corresponding workflow/query exists.
// SchemaVersion: 1
// Additive-only evolution: new instruments may be added; existing instruments/tags must never be
//   removed or renamed once published.

using System.Diagnostics.Metrics;

namespace PlantSafety.Application.Features.SafetyEvents.Metrics;

/// <summary>
/// Domain-specific metrics for Incident Management workflows, using the .NET Meter API
/// (compatible with OpenTelemetry / Prometheus exporters), following the same instrumentation
/// pattern as <see cref="Behaviors.Metrics.MetricsBehavior{TRequest, TResponse}"/>.
/// </summary>
public static class IncidentMetrics
{
    private static readonly Meter _meter = new("PlantSafety.Application.Incidents", "1.0");

    /// <summary>
    /// Counts Incident creation events. Incremented once per successfully persisted Incident,
    /// after the record is written and before the handler returns. No high-cardinality labels
    /// are used, per the canonical metrics naming rule (module.operation.metric).
    /// </summary>
    public static readonly Counter<long> IncidentsCreated = _meter.CreateCounter<long>(
        "plantsafety.incidents.created.total",
        description: "Total number of Incident records created.");
}
