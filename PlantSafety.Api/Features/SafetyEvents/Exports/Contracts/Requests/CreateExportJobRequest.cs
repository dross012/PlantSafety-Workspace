// ============================================================================
// File: CreateExportJobRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for POST /features/safety-events/exports,
//              bound from the request body by the V1 controller.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.Exports.Contracts.Requests;

/// <summary>Request body for queuing an asynchronous safety event data export job.</summary>
/// <param name="ExportType">Type of data to export (e.g., Incidents, NearMisses, Observations).</param>
/// <param name="Format">Export format: csv or json.</param>
/// <param name="FromUtc">Inclusive lower bound on the occurrence date filter.</param>
/// <param name="ToUtc">Inclusive upper bound on the occurrence date filter.</param>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="Status">Optional status filter.</param>
/// <param name="Severity">Optional severity filter.</param>
/// <param name="MaxRecords">Maximum number of records to export (1-10,000).</param>
public sealed record CreateExportJobRequest(
    string          ExportType,
    string          Format,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    Guid?           SiteId,
    string?         Status,
    string?         Severity,
    int             MaxRecords);
