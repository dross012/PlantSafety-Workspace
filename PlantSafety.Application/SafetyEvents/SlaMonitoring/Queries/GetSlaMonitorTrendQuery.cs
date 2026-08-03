// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query returning the SLA compliance trend summary (compliance %, breach
//   count, severity distribution) over a rolling window. Backs
//   v1/safety-events/incidents/sla-monitor/trend (UI Gaps Scoping, IncidentManagement Part 2).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Models;

namespace PlantSafety.Application.SafetyEvents.SlaMonitoring.Queries;

/// <summary>Returns the SLA compliance trend summary for the caller's tenant.</summary>
public sealed record GetSlaMonitorTrendQuery : IQuery<SlaTrendDto>
{
    /// <inheritdoc/>
    public required string TraceId { get; init; }

    /// <inheritdoc/>
    public required string SpanId { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid RequestingUserId { get; init; }

    /// <inheritdoc/>
    public required string RequestingUserRole { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public CachePreference CachePreference { get; init; } = CachePreference.None;

    /// <inheritdoc/>
    public bool SuppressUnauthorized { get; init; }

    /// <inheritdoc/>
    public bool StrictMode { get; init; }

    /// <summary>Rolling trend window, in days. Defaults to 30.</summary>
    public int TrendWindowDays { get; init; } = 30;
}
