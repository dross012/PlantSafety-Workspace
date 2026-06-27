// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Safety Event Export Job record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Models.Exports;

/// <summary>
/// Tracks the lifecycle of an asynchronous export job for safety event data.
/// Export jobs are created, processed in the background, and made available for download.
/// </summary>
public sealed record SafetyEventExportJobDto
{
    /// <summary>Unique identifier of the export job (UUIDv7).</summary>
    public required Guid ExportJobId { get; init; }

    /// <summary>Tenant owning this export job.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Module/aggregate type being exported (e.g., Incident, NearMiss).</summary>
    public required string ExportType { get; init; }

    /// <summary>Requested export format (csv, json).</summary>
    public required string Format { get; init; }

    /// <summary>Job status (Queued, Processing, Completed, Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the export was requested.</summary>
    public required DateTimeOffset RequestedAtUtc { get; init; }

    /// <summary>UTC timestamp when the export was completed, if applicable.</summary>
    public DateTimeOffset? CompletedAtUtc { get; init; }

    /// <summary>Download URL for the completed export file (signed, time-limited).</summary>
    public string? DownloadUrl { get; init; }

    /// <summary>Error message if the job failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Number of records exported.</summary>
    public int? RecordCount { get; init; }
}
