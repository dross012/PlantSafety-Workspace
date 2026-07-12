// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to export Incident records as a structured data payload.
//   Returns a byte array (CSV or JSON) via the Infrastructure export service.
//   Requires SafetyEvents.Incidents.Export permission.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Export;

/// <summary>
/// Exports Incident records to a structured byte payload (CSV or JSON).
/// Scoped to the caller's <see cref="TenantId"/>.
/// The caller must hold <c>SafetyEvents.Incidents.Export</c>.
/// </summary>
public sealed record ExportIncidentsQuery : IQuery<byte[]>
{
    // --- Canonical envelope ---

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

    // --- Export parameters ---

    /// <summary>Export format: <c>csv</c> or <c>json</c>.</summary>
    public string Format { get; init; } = "csv";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive lower bound on occurrence date.</summary>
    public DateTimeOffset? OccurredFromUtc { get; init; }

    /// <summary>Inclusive upper bound on occurrence date.</summary>
    public DateTimeOffset? OccurredToUtc { get; init; }

    /// <summary>Max number of records to export. Clamped to 10,000.</summary>
    public int MaxRecords { get; init; } = 5000;
}
