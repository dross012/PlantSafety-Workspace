// ============================================================================
// File: ExportIncidentsRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for GET /features/safety-events/incidents/export,
//              bound from the query string.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.IncidentManagement.Contracts.Requests;

/// <summary>Query parameters for exporting Incident records.</summary>
public sealed record ExportIncidentsRequest
{
    /// <summary>Export format: <c>csv</c> or <c>json</c>.</summary>
    public string Format { get; init; } = "csv";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive lower bound on occurrence date.</summary>
    public DateTimeOffset? OccurredFromUtc { get; init; }

    /// <summary>Inclusive upper bound on occurrence date.</summary>
    public DateTimeOffset? OccurredToUtc { get; init; }

    /// <summary>Max number of records to export. Clamped to 10,000 by the handler.</summary>
    public int MaxRecords { get; init; } = 5000;
}
